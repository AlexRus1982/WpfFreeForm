using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace ModuleSystem
{
	public class XM_MixerChannel : ModuleMixerChannel
	{

		public XM_PatternChannel patternChannel	= null;
		public XM_Instrument instrument			= null;
		public XM_Instrument lastInstrument		= null;
		public byte volumeEffect				= 0;

		public void FixInstrumentPosition()
        {
			if (instrumentDirection == 1)
			{
				if ((instrumentPosition > instrumentLength) && (loopType == 0x00))
					instrumentPosition = instrumentLength;

				if ((instrumentPosition > instrumentLoopEnd) && (loopType == 0x01))
					instrumentPosition = instrumentLoopStart;

				if ((instrumentPosition > instrumentLoopEnd) && (loopType == 0x02))
				{
					instrumentPosition = instrumentLoopEnd;
					instrumentDirection = -1;
				}
			}

			if (instrumentDirection == -1)
			{
				if ((instrumentPosition < instrumentLoopStart) && (loopType == 0x02))
				{
					instrumentPosition = instrumentLoopStart;
					instrumentDirection = 1;
				}
			}
		}

	} // end of class XM_MixerChannel

	public class XM_Sample 
	{
		public uint orderNumber			= 0;					//
		public uint sampleSizeInBytes	= 0;					//
		public uint sampleLength		= 0;					//
		public uint sampleLoopStart		= 0;					//
		public uint sampleLoopLength	= 0;                    //
		public uint sampleLoopEnd		= 0;					//
		public float sampleVolume		= 0;					//
		public byte sampleFinetune		= 0;					// -128..+127;
		public byte sampleType			= 0;                    // Bit 0-1: 0 = No loop
																//		    1 = Forward loop
																//		    2 = Bidirectional loop(aka ping-pong)
																// Bit 4  : 0 = 8-bit samples
																//		    1 = 16-bit samples
		public byte sampleLoopType		= 0;
		public bool sampleIs16Bits		= false;
		public byte samplePanning		= 0;					// 0..255
		public sbyte relativeNoteNumber	= 0;					// (signed byte)
		public byte packedType			= 0;					// 00 = regular delta packed sample data
																// AD = 4-bit ADPCM-compressed sample data
		public string sampleName		= "";					// 22 - chars
		public List<float> sampleData	= new List<float>();	// 8- or 16-bit packed values
															
		public XM_Sample()
		{
		}
		
		public override string ToString()
		{
			string loopType = "Off";
			loopType = ((sampleType & 0x01) != 0) ? "Forward" : loopType;
			loopType = ((sampleType & 0x02) != 0) ? "Bidirectional" : loopType;
			string res = sampleName.Trim().PadRight(22, '.');

			if (sampleSizeInBytes > 0)
			{
				res += " ( size : " + sampleLength + " byte(s), "
						+ (sampleIs16Bits ? "16-bit" : "8-bit") + ", "
						+ "volume : " + sampleVolume + ", "
						+ "loop : " + sampleType + " = " + loopType + ", ";
				if ((sampleType & 0x03) != 0) res += "loopStart : " + sampleLoopStart + ", " + "loopEnd : " + sampleLoopEnd + ", ";
				res +=  "packed - " + packedType + ((packedType != 0xAD) ? " - regular delta" : " - ADPCM-compressed") + ")";
			}

			return res;
			//*/
			//return this.ToShortString();
		}

		public string ToShortString()
		{
			return sampleName;
		}

		public void readHeadersFromStream(Stream stream)
		{
			long startSamplePosition	= stream.Position;

			sampleSizeInBytes			= ModuleUtils.ReadDWord(stream);
			sampleLoopStart				= ModuleUtils.ReadDWord(stream);
			sampleLoopLength			= ModuleUtils.ReadDWord(stream);
			byte vol					= ModuleUtils.ReadByte(stream);
			sampleVolume				= (vol > 64) ? 1.0f : (float)vol / 64.0f;
			sampleFinetune				= ModuleUtils.ReadByte(stream);
			sampleType					= ModuleUtils.ReadByte(stream);
			samplePanning				= ModuleUtils.ReadByte(stream);
			relativeNoteNumber			= (sbyte)ModuleUtils.ReadSignedByte(stream);
			packedType					= ModuleUtils.ReadByte(stream);
			sampleName					= ModuleUtils.ReadString0(stream, 22);

			sampleLength = ((sampleType & 0x10) != 0) ? sampleSizeInBytes / 2 : sampleSizeInBytes;
			sampleLoopType = (byte)(sampleType & 0x03);
			sampleIs16Bits = ((sampleType & 0x10) != 0);
			if (sampleIs16Bits)
            {
				sampleLoopStart = sampleLoopStart >> 1;
				sampleLoopEnd = sampleLoopEnd >> 1;
			}
			sampleLoopStart = (sampleLoopStart > sampleLength) ? sampleLength : sampleLoopStart;
			sampleLoopEnd = sampleLoopStart + sampleLoopLength;
			sampleLoopEnd = (sampleLoopEnd > sampleLength) ? sampleLength : sampleLoopEnd;

			relativeNoteNumber = 0;
			//stream.Seek(startSamplePosition + 40, SeekOrigin.Begin);
		}

		// NOTE : this function load regular delta packed sample data
		// TODO : need to do loading of AD = 4-bit ADPCM-compressed sample data
		public void readDataFromStream(Stream stream)
		{
			sampleData.Clear();
			float ampDivider = ((sampleType & 0x10) != 0) ? 0.000030517578125f /* 1/32768 */ : 0.0078125f /* 1/128 */;
			if (sampleLength > 0)
			{
				float oldValue = 0;
				for (uint i = 0; i < sampleLength; i++)
				{
					float sampleValue;
					sampleValue = ((sampleType & 0x10) != 0) ? ModuleUtils.ReadWord(stream) : ModuleUtils.ReadSignedByte(stream);
					oldValue += sampleValue * ampDivider;
					if (oldValue < -1) oldValue += 2;
					else if (oldValue > 1) oldValue -= 2;
					sampleData.Add(oldValue);
				}
			}
		}
		public void Clear()
		{
			sampleData.Clear();
		}

	} // end of class XM_Sample

	public class XM_Instrument 
	{
		public uint instrumentSize						= 0;
		public string instrumentName					= "";
		public byte instrumentType						= 0;
		public uint samplesNumber						= 0;

		public uint headerSize							= 0;
		public List<byte> keymapAssignements			= new List<byte>();
		public List<ushort> pointsForVolumeEnvelope		= new List<ushort>();
		public List<ushort> pointsForPanningEnvelope	= new List<ushort>();
		public byte numberOfVolumePoints				= 0;
		public byte numberOfPanningPoints				= 0;
		public byte volumeSustainPoint					= 0;
		public byte volumeLoopStartPoint				= 0;
		public byte volumeLoopEndPoint					= 0;
		public byte panningSustainPoint					= 0;
		public byte panningLoopStartPoint				= 0;
		public byte panningLoopEndPoint					= 0;
		public byte volumeType							= 0; // bit 0: On; 1: Sustain; 2: Loop;
		public byte panningType							= 0; // bit 0: On; 1: Sustain; 2: Loop;
		public byte vibratoType							= 0;
		public byte vibratoSweep						= 0;
		public byte vibratoDepth						= 0;
		public byte vibratoRate							= 0;
		public ushort volumeFadeout						= 0;
		public List<byte> reserved						= new List<byte>(22); // reserved 22 bytes
		public List<XM_Sample> samples					= new List<XM_Sample>();
		public XM_Sample sample							= null;

		public XM_Instrument()
		{
		}

		public override string ToString()
		{
			string res = instrumentName.Trim().PadRight(22, '.');
			if (samples.Count > 0)
			{
				res += " ( sample(s) : ";
				foreach (XM_Sample sample in samples) res += sample.orderNumber + " ";
				res += ")";
			}
			return res;
		}

		public string ToShortString()
		{
			return instrumentName;
		}

		public void ReadFromStream(Stream stream, ref uint sampleOrder)
		{
			long startInstrumentPosition	= stream.Position;
			instrumentSize					= ModuleUtils.ReadDWord(stream);
			instrumentName					= ModuleUtils.ReadString0(stream, 22);
			instrumentType					= ModuleUtils.ReadByte(stream); // Length
			samplesNumber					= ModuleUtils.ReadWord(stream);
			samples.Clear();

			if (samplesNumber > 0)
			{
				headerSize						= ModuleUtils.ReadDWord(stream);
				for (uint i = 0; i < 96; i++)	keymapAssignements.Add(ModuleUtils.ReadByte(stream));
				for (uint i = 0; i < 24; i++)	pointsForVolumeEnvelope.Add(ModuleUtils.ReadWord(stream));
				for (uint i = 0; i < 24; i++)	pointsForPanningEnvelope.Add(ModuleUtils.ReadWord(stream));
				numberOfVolumePoints			= ModuleUtils.ReadByte(stream);
				numberOfPanningPoints			= ModuleUtils.ReadByte(stream);
				volumeSustainPoint				= ModuleUtils.ReadByte(stream);
				volumeLoopStartPoint			= ModuleUtils.ReadByte(stream);
				volumeLoopEndPoint				= ModuleUtils.ReadByte(stream);
				panningSustainPoint				= ModuleUtils.ReadByte(stream);
				panningLoopStartPoint			= ModuleUtils.ReadByte(stream);
				panningLoopEndPoint				= ModuleUtils.ReadByte(stream);
				volumeType						= ModuleUtils.ReadByte(stream); // bit 0: On; 1: Sustain; 2: Loop;
				panningType						= ModuleUtils.ReadByte(stream); // bit 0: On; 1: Sustain; 2: Loop;
				vibratoType						= ModuleUtils.ReadByte(stream);
				vibratoSweep					= ModuleUtils.ReadByte(stream);
				vibratoDepth					= ModuleUtils.ReadByte(stream);
				vibratoRate						= ModuleUtils.ReadByte(stream);
				volumeFadeout					= ModuleUtils.ReadWord(stream);
				//for (uint i = 0; i < 22; i++)	reserved.Add(ModuleUtils.ReadByte(stream));

				//System.Diagnostics.Debug.WriteLine("Seek : " + stream.Position + " " + (instrumentSize - 243) + "\n");
				stream.Seek(instrumentSize - 241, SeekOrigin.Current);

				long sampleDataLength = 0;
				for (uint i = 0; i < samplesNumber; i++)
				{
					var sample = new XM_Sample();
					sample.readHeadersFromStream(stream);
					sample.orderNumber = sampleOrder++;
					samples.Add(sample);
					sampleDataLength += sample.sampleSizeInBytes;
				}
				
				long startSampleDataPosition = stream.Position;
				foreach (XM_Sample sample in samples)
				{
					sample.readDataFromStream(stream);
				}
				stream.Seek(startSampleDataPosition + sampleDataLength, SeekOrigin.Begin);
			}
			else stream.Seek(startInstrumentPosition + instrumentSize, SeekOrigin.Begin);

			sample = (samples.Count > 0) ? samples[0] : null;
		}
		
		public void Clear()
		{
			samples.Clear();
		}

	} // end of class XM_Instrument

	public class XM_PatternChannel 
	{

		public uint period						= 0;
		public byte noteIndex					= 0;
		public byte instrument					= 0;
		public byte volumeEffect				= 0;
		public byte effekt						= 0;
		public byte effektOp					= 0;
		public bool cutNote						= false;
		public static string[] effectStrings	=
		{
			 "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", 
			 "A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
			 "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
			 "U", "V", "W", "X"
		};

		private static string GetNoteNameToIndex(byte index)
		{
			if (index == 0) return "...";  // No Note
			if (index == 97) return "==="; // Note cut value
			return (ModuleConst.noteStrings[(index - 1) % 12] + ((index - 1) / 12 + 1));
		}

		private static string getVolumeEffectString(byte volumeEffect)
		{
			if (volumeEffect >= 0x00 && volumeEffect <= 0x0F) return "...";  // do nothing
			if (volumeEffect >= 0x10 && volumeEffect <= 0x4F) return "V" + ModuleUtils.GetAsDec(volumeEffect - 0x10, 2);  // v 0..63
			if (volumeEffect == 0x50) return "V64";  // v 64
			if (volumeEffect >= 0x51 && volumeEffect <= 0x5F) return "...";  // undefined
			if (volumeEffect >= 0x60 && volumeEffect <= 0x6F) return "D" + ModuleUtils.GetAsDec(volumeEffect - 0x60, 2);  // volumeEffect slide down
			if (volumeEffect >= 0x70 && volumeEffect <= 0x7F) return "C" + ModuleUtils.GetAsDec(volumeEffect - 0x70, 2);  // volumeEffect slide up
			if (volumeEffect >= 0x80 && volumeEffect <= 0x8F) return "B" + ModuleUtils.GetAsDec(volumeEffect - 0x80, 2);  // Fine volumeEffect down
			if (volumeEffect >= 0x90 && volumeEffect <= 0x9F) return "A" + ModuleUtils.GetAsDec(volumeEffect - 0x90, 2);  // Fine volumeEffect up
			if (volumeEffect >= 0xA0 && volumeEffect <= 0xAF) return "U" + ModuleUtils.GetAsDec(volumeEffect - 0xA0, 2);  // Vibrato speed
			if (volumeEffect >= 0xB0 && volumeEffect <= 0xBF) return "H" + ModuleUtils.GetAsDec(volumeEffect - 0xB0, 2);  // Vibrato deph
			if (volumeEffect >= 0xC0 && volumeEffect <= 0xCF) return "P" + ModuleUtils.GetAsDec((volumeEffect - 0xC0) * 4 + 2, 2);  // Set panning (2,6,10,14..62)
			if (volumeEffect >= 0xD0 && volumeEffect <= 0xDF) return "L" + ModuleUtils.GetAsDec(volumeEffect - 0xD0, 2);  // Pan slide left
			if (volumeEffect >= 0xE0 && volumeEffect <= 0xEF) return "R" + ModuleUtils.GetAsDec(volumeEffect - 0xE0, 2);  // Pan slide right
			if (volumeEffect >= 0xF0 && volumeEffect <= 0xFF) return "G" + ModuleUtils.GetAsDec(volumeEffect - 0xF0, 2);  // Tone portamento
			return "...";
		}

		public override string ToString()
		{
			string res = GetNoteNameToIndex(noteIndex);
			//res += ((period == 0 && noteIndex != 0) || (period != 0 && noteIndex == 0)) ? "!" : " ";
			res += " " + ((instrument != 0) ? ModuleUtils.GetAsDec(instrument, 2) : "..");
			res += " " + getVolumeEffectString(volumeEffect) + " ";
			res += ((effekt != 0) && (effekt < 34)) ? effectStrings[effekt] + ModuleUtils.GetAsHex(effektOp, 2) : "...";
			return res;
		}

	} // end of class XM_PatternChannel

	public class XM_PatternRow 
	{

		public List<XM_PatternChannel> patternChannels = new List<XM_PatternChannel>();
		
		public override string ToString()
		{
			string res = "";
			for (int i = 0; i < patternChannels.Count; i++)
				res += patternChannels[i].ToString() + '|';
			return res;
		}

		public void Clear()
		{
			patternChannels.Clear();
		}

	} // end of class XM_PatternRow

	public class XM_Pattern 
	{

		private uint headerLength				= 0;
		private byte packingType				= 0;
		private uint numberOfRows				= 0;
		private uint packedSize					= 0;

		public List<XM_PatternRow> patternRows = new List<XM_PatternRow>();
		
		private XM_PatternChannel CreateNewPatternChannel(Stream stream, uint numberOfSamples)
		{
			XM_PatternChannel channel = new XM_PatternChannel();
			byte b0 = ModuleUtils.ReadByte(stream);
			if ((b0 & 0x80) != 0)
            {
				if ((b0 & 0x01) != 0) channel.noteIndex		= ModuleUtils.ReadByte(stream);
				if ((b0 & 0x02) != 0) channel.instrument	= ModuleUtils.ReadByte(stream);
				if ((b0 & 0x04) != 0) channel.volumeEffect	= ModuleUtils.ReadByte(stream);
				if ((b0 & 0x08) != 0) channel.effekt		= ModuleUtils.ReadByte(stream);
				if ((b0 & 0x10) != 0) channel.effektOp		= ModuleUtils.ReadByte(stream);
			}
			else
            {
				channel.noteIndex		= (byte)(b0 & 0x7F);
				channel.instrument		= ModuleUtils.ReadByte(stream);
				channel.volumeEffect	= ModuleUtils.ReadByte(stream);
				channel.effekt			= ModuleUtils.ReadByte(stream);
				channel.effektOp		= ModuleUtils.ReadByte(stream);
			}
			if (channel.noteIndex > 0 && channel.noteIndex < 97 && channel.volumeEffect == 0) channel.volumeEffect = 0x50;
			return channel;
		}

		public void ReadPatternData(Stream stream, string moduleID, uint numberOfChannels, uint numberOfSamples)
		{
			headerLength = ModuleUtils.ReadDWord(stream);
			packingType = ModuleUtils.ReadByte(stream);
			numberOfRows = ModuleUtils.ReadWord(stream);
			packedSize = ModuleUtils.ReadWord(stream);

			long patternEndPosition = stream.Position + packedSize;

			patternRows.Clear();
			for (int row = 0; row < numberOfRows; row++)
			{
				XM_PatternRow pRow = new XM_PatternRow();
				for (int channel = 0; channel < numberOfChannels; channel++)
					pRow.patternChannels.Add(CreateNewPatternChannel(stream, numberOfSamples));

				patternRows.Add(pRow);
			}
			
			stream.Seek(patternEndPosition, SeekOrigin.Begin);
		}

		public override string ToString()
		{
			string res = "\n";
			res += "\n" + "headerLength : " + headerLength;
			res += "\n" + "packingType : " + packingType;
			res += "\n" + "numberOfRows : " + numberOfRows;
			res += "\n" + "packedSize : " + packedSize;
			res += "\n";

			if (patternRows[0] != null)
			{
				string ln = "====";
				for (int i = 0; i < patternRows[0].patternChannels.Count; i++)
					ln += "===============";

				res += ln + "\n";

				for (int i = 0; i < patternRows.Count; i++)
					res += ModuleUtils.GetAsHex((uint)i, 2) + ":|" + patternRows[i] + "\n";

				res += ln + "\n";
			}
			else res += "empty pattern\n";
			return res;
		}

		public void Clear()
		{
			patternRows.Clear();
		}

	} // end of class XM_Pattern

	public class XM_Module : Module
    {

		private byte XM_1A_Byte					= 0;
		private uint XM_Flags					= 0;

		public List<XM_Instrument> instruments	= new List<XM_Instrument>();
		public List<XM_Pattern> patterns		= new List<XM_Pattern>();
		public List<uint> arrangement			= new List<uint>();

		private uint sampleOrderNum				= 1;
		private XM_Mixer mixer					= null;
		
		public XM_Module():base("XM format")
		{
			DebugMes("XM Sound Module created");			
		}
		
		public string InstrumentsToString()
		{
			string res = "Instruments info : \n";
			uint i = 1;
			foreach (XM_Instrument inst in instruments)	
				res += ModuleUtils.GetAsDec((int)i++, 3) + " : " + inst.ToString().Trim() + "\n";
			return res;
		}
		
		public string SamplesToString()
		{
			string res = "Samples info : \n";
			uint i = 1;
			foreach (XM_Instrument inst in instruments)
				foreach (XM_Sample sample in inst.samples)
					res += ModuleUtils.GetAsDec((int)i++, 3) + " : " + sample.ToString().Trim() + "\n";
			return res;
		}

		public override bool CheckFormat(Stream stream)
		{
			byte[] bytesID = new byte[17];
			stream.Seek(0, SeekOrigin.Begin);
			stream.Read(bytesID);
			stream.Seek(37, SeekOrigin.Begin);
			XM_1A_Byte = (byte)stream.ReadByte();
			moduleID = Encoding.ASCII.GetString(bytesID);
			return moduleID == "Extended Module: " && XM_1A_Byte == 0x1A;
		}

		private void ReadArrangement(Stream stream)
		{
			arrangement.Clear();
			for (int i = 0; i < 256; i++)
			{
				byte patNum = ModuleUtils.ReadByte(stream);
				if (i < songLength) arrangement.Add(patNum);
			}
		}
		
		private void ReadInstruments(Stream stream)
		{
			sampleOrderNum = 1;
			foreach (XM_Instrument inst in instruments) inst.Clear();				
			instruments.Clear();
			for (uint i = 0; i < numberOfInstruments; i++)
			{
				XM_Instrument inst = new XM_Instrument();
				inst.ReadFromStream(stream, ref sampleOrderNum);
				instruments.Add(inst);
			}
		}
		
		private void ReadPatterns(Stream stream)
		{
			foreach (XM_Pattern pat in patterns)
				pat.Clear();

			patterns.Clear();
            for (int i = 0; i < numberOfPatterns; i++)						// Read the patterndata
			{
                XM_Pattern pattern = new XM_Pattern();
                pattern.ReadPatternData(stream, moduleID, numberOfChannels, numberOfSamples);
                patterns.Add(pattern);
				DebugMes("Patern : " + i + pattern.ToString());
            }
        }

		public override bool ReadFromStream(Stream stream)
		{
			if (!CheckFormat(stream)) return false;

			fileLength = stream.Length;
			baseVolume = 1.0f;
			BPM = 125;
			tempo = 6;

			stream.Seek(0, SeekOrigin.Begin);
			moduleID = ModuleUtils.ReadString0(stream, 17);
			songName = ModuleUtils.ReadString0(stream, 20);
			XM_1A_Byte = (byte)stream.ReadByte();
			trackerName = ModuleUtils.ReadString0(stream, 20);
			version = ModuleUtils.ReadWord(stream);
			headerSize = (uint)ModuleUtils.ReadDWord(stream);
			songLength = ModuleUtils.ReadWord(stream);
			songRepeatPosition = ModuleUtils.ReadWord(stream);
			numberOfChannels = ModuleUtils.ReadWord(stream);
			numberOfPatterns = ModuleUtils.ReadWord(stream);
			numberOfInstruments = ModuleUtils.ReadWord(stream);
			XM_Flags = (uint)ModuleUtils.ReadWord(stream);
			tempo = ModuleUtils.ReadWord(stream);
			BPM = ModuleUtils.ReadWord(stream);

			ReadArrangement(stream);        // read pattern order
			ReadPatterns(stream);           // read patterns
			ReadInstruments(stream);        // read instruments

			DebugMes(InstrumentsToString());
			DebugMes(SamplesToString());

			mixer = new XM_Mixer(this);
			return true;
		}
		
		public override string ToString()
		{
			string modInfo = "";
			modInfo += "\n" + "moduleID : " + moduleID;
			modInfo += "\n" + "songName : " + songName;
			modInfo += "\n" + "XM_1A_Byte : " + XM_1A_Byte;
			modInfo += "\n" + "trackerName : " + trackerName;
			modInfo += "\n" + "version : " + version;
			modInfo += "\n" + "headerSize : " + headerSize;
			modInfo += "\n" + "songLength : " + songLength;
			modInfo += "\n" + "songRepeatPosition : " + songRepeatPosition;
			modInfo += "\n" + "numberOfChannels : " + numberOfChannels;
			modInfo += "\n" + "numberOfPatterns : " + numberOfPatterns;
			modInfo += "\n" + "numberOfInstruments : " + numberOfInstruments;
			modInfo += "\n" + "XM_Flags : " + XM_Flags;
			modInfo += "\n" + "tempo : " + tempo;
			modInfo += "\n" + "BPM : " + BPM + "\n";
			for (int i = 0; i < songLength; i++) modInfo += arrangement[i] + ((i < songLength - 1) ? "," : "");
			return modInfo + "\n\n";
		}
		
		public override void Play()
        {
			mixer.PlayModule(0);
        }
		
		public override void Stop()
		{
			mixer.Stop();
		}
		
		public override void Dispose()
		{
			mixer.Stop();
		}

	} // end of class XM_Module
	
	public class XM_Mixer : ModuleMixer
	{

		private XM_Module module								= null;
		private XM_Pattern pattern								= null;
		private List<XM_MixerChannel> mixChannels				= new List<XM_MixerChannel>();

		private List<Func<XM_MixerChannel, bool>> noteEffects	= new List<Func<XM_MixerChannel, bool>>();
		private List<bool> noteEffectsUsed						= new List<bool>();
		private List<Func<XM_MixerChannel, bool>> tickEffects	= new List<Func<XM_MixerChannel, bool>>();
		private List<bool> tickEffectsUsed						= new List<bool>();
		private List<Func<XM_MixerChannel, bool>> effectsE		= new List<Func<XM_MixerChannel, bool>>();
		private List<bool> effectsEUsed							= new List<bool>();

		private WaveOutEvent waveOut							= null;
		private ModuleSoundStream waveStream					= null;
		private WaveFileReader waveReader						= null;
		private Task MixingTask									= null;

		#region Effects

		private bool NoEffect(XM_MixerChannel mc)               // no effect
		{
			return false;
		}
		
		private bool NoteEffect0(XM_MixerChannel mc)                // Arpeggio
		{
			if (mc.effectArg == 0) return false;
			mc.arpeggioCount = 0;
			mc.arpeggioIndex = mc.noteIndex;
			mc.arpeggioX = mc.arpeggioIndex + mc.effectArgX;
			mc.arpeggioY = mc.arpeggioIndex + mc.effectArgY;
			mc.arpeggioPeriod0 = GetNoteFreq(mc.arpeggioIndex, /*mc.currentFineTune*/ 0);
			mc.period = mc.arpeggioPeriod0;
			mc.arpeggioPeriod1 = (mc.arpeggioX < 96) ? GetNoteFreq(mc.arpeggioX, /*mc.currentFineTune*/ 0) : mc.arpeggioPeriod0;
			mc.arpeggioPeriod2 = (mc.arpeggioY < 96) ? GetNoteFreq(mc.arpeggioY, /*mc.currentFineTune*/ 0) : mc.arpeggioPeriod0;
			return true;
		}
		
		private bool NoteEffect1(XM_MixerChannel mc)                // Slide up (Portamento Up)
		{
			mc.portamentoStart = (int)mc.period;
			mc.portamentoStep = -(int)mc.effectArg;
			return true;
		}
		
		private bool NoteEffect2(XM_MixerChannel mc)             // Slide down (Portamento Down)
		{
			mc.portamentoStart = (int)mc.period;
			mc.portamentoStep = (int)mc.effectArg;
			return true;
		}
		
		private bool NoteEffect3(XM_MixerChannel mc)             // Slide to note
		{
			if (mc.effectArg != 0)
			{
				mc.lastPortamentoStep = mc.portamentoStep;
				mc.portamentoStep = (mc.portamentoStart <= mc.portamentoEnd) ? (int)mc.effectArg : -(int)mc.effectArg;
			}
			else
				mc.portamentoStep = mc.lastPortamentoStep;

			return true;
		}
		
		private bool NoteEffect4(XM_MixerChannel mc)             // Vibrato
		{
			mc.vibratoStart = (int)mc.period;
			mc.vibratoPeriod = mc.vibratoStart;
			if (mc.effectArgX != 0 && mc.effectArgY != 0)
			{
				mc.lastVibratoFreq = (int)mc.effectArgX;
				mc.lastVibratoAmp = (int)mc.effectArgY;
				mc.vibratoFreq = (int)mc.effectArgX;
				mc.vibratoAmp = (int)mc.effectArgY;
				mc.vibratoCount = 0;
			}
			else
			{
				mc.vibratoFreq = mc.lastVibratoFreq;
				mc.vibratoAmp = mc.lastVibratoAmp;
				mc.vibratoCount = 0;
			}
			return true;
		}
		
		private bool NoteEffect5(XM_MixerChannel mc)             // Continue Slide to note + Volume slide
		{
			NoteEffectA(mc);
			return true;
		}
		
		private bool NoteEffect6(XM_MixerChannel mc)             // Continue Vibrato + Volume Slide
		{
			NoteEffectA(mc);
			mc.effectArgX = 0;
			mc.effectArgY = 0;
			NoteEffect4(mc);
			return true;
		}
		
		private bool NoteEffect7(XM_MixerChannel mc)             // Tremolo
		{
			mc.tremoloCount = (mc.tremoloType <= 0x03) ? 0 : mc.tremoloCount;
			mc.tremoloStart = mc.channelVolume;
			mc.tremoloFreq = (mc.effectArgX != 0) ? (int)mc.effectArgX : 0;
			mc.tremoloAmp = (mc.effectArgY != 0) ? (int)mc.effectArgY : 0;
			return true;
		}
		
		private bool NoteEffect8(XM_MixerChannel mc)             // Not Used
		{
			return true;
		}
		
		private bool NoteEffect9(XM_MixerChannel mc)             // Set Sample Offset
		{
			mc.instrumentPosition = mc.effectArg << 8;
			mc.FixInstrumentPosition();
			return true;
		}
		
		private bool NoteEffectA(XM_MixerChannel mc)             // Volume Slide
		{
			mc.volumeSlideStep = 0;
			if (mc.effectArgX != 0)			mc.volumeSlideStep = (float)mc.effectArgX / 0x40;   // Volume Slide up
			else if (mc.effectArgY != 0)	mc.volumeSlideStep = -(float)mc.effectArgY / 0x40;  // Volume Slide down
			mc.channelVolume -= mc.volumeSlideStep;
			return true;
		}
		
		private bool NoteEffectB(XM_MixerChannel mc)             // Position Jump
		{
			mc.patternNumToJump = mc.effectArgX * 16 + mc.effectArgY;
			if (mc.patternNumToJump > 0x7F) mc.patternNumToJump = 0;

			currentRow = 0;
			track = module.arrangement[(int)mc.patternNumToJump];
			pattern = module.patterns[(int)module.arrangement[(int)track]];
			return true;
		}
		
		private bool NoteEffectC(XM_MixerChannel mc)             // Set Volume
		{
			mc.channelVolume = (float)mc.effectArg / 0x40;
			mc.channelVolume = (mc.channelVolume > 1.0f) ? 1.0f : mc.channelVolume;
			return true;
		}
		
		private bool NoteEffectD(XM_MixerChannel mc)             // Pattern Break
		{
			mc.positionToJump = mc.effectArgX * 10 + mc.effectArgY;
			if (mc.positionToJump > 0x3F) mc.positionToJump = 0;

			track++;
			pattern = module.patterns[(int)module.arrangement[(int)track]];
			currentRow = mc.positionToJump;

			return true;
		}
		
		private bool NoteEffectE(XM_MixerChannel mc)             // Extended Effects
		{
			effectsE[(int)mc.effectArgX](mc);
			effectsEUsed[(int)mc.effectArgX] = true;
			return true;
		}
		
		private bool NoteEffectF(XM_MixerChannel mc)             // SetSpeed
		{
			if ((mc.effectArg >= 0x20) && (mc.effectArg <= 0xFF))
			{
				BPM = mc.effectArg;
				SetBPM();
			}
			if ((mc.effectArg > 0) && (mc.effectArg <= 0x1F)) speed = mc.effectArg;

			//System.Diagnostics.Debug.WriteLine("Set BPM/speed -> " + BPM + " : " + speed);

			return true;
		}
		//----------------------------------------------------------------------------------------------
		
		private bool TickEffect0(XM_MixerChannel mc)
		{
			if (mc.effectArg == 0) return false;
			uint arpeggioPeriod = mc.arpeggioPeriod0;
			arpeggioPeriod = (mc.arpeggioCount == 1) ? mc.arpeggioPeriod1 : arpeggioPeriod;
			arpeggioPeriod = (mc.arpeggioCount == 2) ? mc.arpeggioPeriod2 : arpeggioPeriod;
			//mc.period = arpeggioPeriod;
			mc.periodInc = CalcPeriodIncrement(arpeggioPeriod);
			mc.arpeggioCount = (mc.arpeggioCount + 1) % 4;
			return true;
		}
		
		private bool TickEffect1(XM_MixerChannel mc)
		{
			mc.period = (uint)mc.portamentoStart;
			mc.periodInc = CalcClampPeriodIncrement(mc);
			mc.portamentoStart += mc.portamentoStep;
			return true;
		}
		
		private bool TickEffect2(XM_MixerChannel mc)
		{
			mc.period = (uint)mc.portamentoStart;
			mc.periodInc = CalcClampPeriodIncrement(mc);
			mc.portamentoStart += mc.portamentoStep;
			return true;
		}
		
		private bool TickEffect3(XM_MixerChannel mc)
		{

			mc.period = (uint)mc.portamentoStart;
			mc.periodInc = CalcClampPeriodIncrement(mc);

			mc.portamentoStart += mc.portamentoStep;
			if (mc.portamentoStep < 0)
				mc.portamentoStart = (mc.portamentoStart > mc.portamentoEnd) ? mc.portamentoStart : mc.portamentoEnd;
			else
				mc.portamentoStart = (mc.portamentoStart < mc.portamentoEnd) ? mc.portamentoStart : mc.portamentoEnd;

			return true;
		}
		
		private bool TickEffect4(XM_MixerChannel mc)
		{
			mc.vibratoAdd = (mc.vibratoType % 4 == 0) ? ModuleConst.ModSinusTable[mc.vibratoCount] : mc.vibratoAdd;
			mc.vibratoAdd = (mc.vibratoType % 4 == 1) ? ModuleConst.ModRampDownTable[mc.vibratoCount] : mc.vibratoAdd;
			mc.vibratoAdd = (mc.vibratoType % 4 == 2) ? ModuleConst.ModSquareTable[mc.vibratoCount] : mc.vibratoAdd;
			mc.vibratoAdd = (mc.vibratoType % 4 == 3) ? ModuleConst.ModRandomTable[mc.vibratoCount] : mc.vibratoAdd;

			mc.periodInc = CalcPeriodIncrement((uint)mc.vibratoPeriod);
			mc.vibratoPeriod = mc.vibratoStart + (int)(mc.vibratoAdd * ((float)mc.vibratoAmp / 128.0f));
			mc.vibratoCount = (mc.vibratoCount + mc.vibratoFreq) & 0x3F;
			return true;
		}
		
		private bool TickEffect5(XM_MixerChannel mc)
		{
			TickEffectA(mc);
			TickEffect3(mc);
			return true;
		}
		
		private bool TickEffect6(XM_MixerChannel mc)
		{
			TickEffectA(mc);
			TickEffect4(mc);
			return true;
		}
		
		private bool TickEffect7(XM_MixerChannel mc)
		{
			mc.tremoloAdd = (mc.tremoloType % 4 == 0) ? ModuleConst.ModSinusTable[mc.tremoloCount] : mc.tremoloAdd;
			mc.tremoloAdd = (mc.tremoloType % 4 == 1) ? ModuleConst.ModRampDownTable[mc.tremoloCount] : mc.tremoloAdd;
			mc.tremoloAdd = (mc.tremoloType % 4 == 2) ? ModuleConst.ModSquareTable[mc.tremoloCount] : mc.tremoloAdd;
			mc.tremoloAdd = (mc.tremoloType % 4 == 3) ? ModuleConst.ModRandomTable[mc.tremoloCount] : mc.tremoloAdd;

			mc.channelVolume = mc.tremoloStart + ((float)mc.tremoloAdd / 64) * ((float)mc.tremoloAmp / 0x40);
			mc.channelVolume = (mc.channelVolume < 0.0f) ? 0.0f : mc.channelVolume;
			mc.channelVolume = (mc.channelVolume > 1.0f) ? 1.0f : mc.channelVolume;
			mc.tremoloCount = (mc.tremoloCount + mc.tremoloFreq) & 0x3F;
			return true;
		}
		
		private bool TickEffect8(XM_MixerChannel mc)
		{
			return true;
		}
		
		private bool TickEffect9(XM_MixerChannel mc)
		{
			return true;
		}
		
		private bool TickEffectA(XM_MixerChannel mc)             // Volume Slide tick
		{
			mc.channelVolume += mc.volumeSlideStep;
			mc.channelVolume = (mc.channelVolume < 0.0f) ? 0.0f : mc.channelVolume;
			mc.channelVolume = (mc.channelVolume > 1.0f) ? 1.0f : mc.channelVolume;
			return true;
		}

		private bool TickEffectB(XM_MixerChannel mc)
		{
			return true;
		}

		private bool TickEffectC(XM_MixerChannel mc)
		{
			return true;
		}

		private bool TickEffectD(XM_MixerChannel mc)
		{
			return true;
		}

		private bool TickEffectE(XM_MixerChannel mc)
		{
			return true;
		}

		private bool TickEffectF(XM_MixerChannel mc)
		{
			return true;
		}
		//----------------------------------------------------------------------------------------------
		
		private bool EffectE0(XM_MixerChannel mc)
		{
			return true;
		}
		
		private bool EffectE1(XM_MixerChannel mc)
		{
			mc.period -= mc.effectArgY;
			mc.periodInc = CalcClampPeriodIncrement(mc);
			return true;
		}
		
		private bool EffectE2(XM_MixerChannel mc)
		{
			mc.period += mc.effectArgY;
			mc.periodInc = CalcClampPeriodIncrement(mc);
			return true;
		}
		
		private bool EffectE3(XM_MixerChannel mc)
		{
			return true;
		}
		
		private bool EffectE4(XM_MixerChannel mc)
		{
			mc.vibratoType = mc.effectArgY & 0x7;
			return true;
		}
		
		private bool EffectE5(XM_MixerChannel mc)
		{
			mc.currentFineTune = (int)mc.effectArgY;
			return true;
		}
		
		private bool EffectE6(XM_MixerChannel mc)
		{
			return true;
		}
		
		private bool EffectE7(XM_MixerChannel mc)
		{
			mc.tremoloType = mc.effectArgY & 0x7;
			return true;
		}
		
		private bool EffectE8(XM_MixerChannel mc)
		{
			return true;
		}
		
		private bool EffectE9(XM_MixerChannel mc)                // Retrigger Sample
		{
			return true;
		}
		
		private bool EffectEA(XM_MixerChannel mc)                // Fine Volume Slide Up
		{
			mc.effect = 0x0A;
			mc.volumeSlideStep = (float)mc.effectArgY / 0x40;
			return true;
		}
		
		private bool EffectEB(XM_MixerChannel mc)                // Fine Volume Slide Down
		{
			mc.effect = 0x0A;
			mc.volumeSlideStep = -(float)mc.effectArgY / 0x40;
			return true;
		}
		
		private bool EffectEC(XM_MixerChannel mc)                // Cut Sample
		{
			return true;
		}
		
		private bool EffectED(XM_MixerChannel mc)
		{
			return true;
		}
		
		private bool EffectEE(XM_MixerChannel mc)                // Delay
		{
			patternDelay = mc.effectArgY;
			return true;
		}
		
		private bool EffectEF(XM_MixerChannel mc)
		{
			return true;
		}
		//----------------------------------------------------------------------------------------------
		#endregion
		
		public XM_Mixer(XM_Module module)
		{
			this.module = module;
			for (int i = 0; i < 16; i++)
			{
				noteEffectsUsed.Add(false);
				tickEffectsUsed.Add(false);
				effectsEUsed.Add(false);
			}

			/*
			for (int i = 0; i < 16; i++)
            {
				noteEffects.Add(NoEffect);
				tickEffects.Add(NoEffect);
				effectsE.Add(NoEffect);
			}
			return; // without effects;
			*/
			
			noteEffects.Add(NoteEffect0);               // 0. ARPEGGIO
			noteEffects.Add(NoteEffect1);               // 1. PORTAMENTO UP 
			noteEffects.Add(NoteEffect2);               // 2. PORTAMENTO DOWN
			noteEffects.Add(NoteEffect3);               // 3. TONE PORTAMENTO
			noteEffects.Add(NoteEffect4);               // 4. VIBRATO
			noteEffects.Add(NoteEffect5);               // 5. TONE PORTAMENTO + VOLUME SLIDE
			noteEffects.Add(NoteEffect6);               // 6. VIBRATO + VOLUME SLIDE
			noteEffects.Add(NoteEffect7);               // 7. TREMOLO
			noteEffects.Add(/*NoteEffect8*/NoEffect);   // 8. PAN
			noteEffects.Add(/*NoteEffect9*/NoEffect);   // 9. SAMPLE OFFSET
			noteEffects.Add(NoteEffectA);               // A. VOLUME SLIDE
			noteEffects.Add(/*NoteEffectB*/NoEffect);   // B. POSITION JUMP
			noteEffects.Add(NoteEffectC);               // C. SET VOLUME
			noteEffects.Add(NoteEffectD);               // D. PATTERN BREAK
			noteEffects.Add(NoteEffectE);               // E. EXTEND EFFECTS
			noteEffects.Add(NoteEffectF);               // F. SET SPEED

			tickEffects.Add(TickEffect0);               // tick effect 0. ARPEGGIO
			tickEffects.Add(TickEffect1);               // tick effect 1. PORTAMENTO UP 
			tickEffects.Add(TickEffect2);               // tick effect 2. PORTAMENTO DOWN
			tickEffects.Add(TickEffect3);               // tick effect 3. TONE PORTAMENTO
			tickEffects.Add(TickEffect4);               // tick effect 4. VIBRATO
			tickEffects.Add(TickEffect5);               // tick effect 5. TONE PORTAMENTO + VOLUME SLIDE
			tickEffects.Add(TickEffect6);               // tick effect 6. VIBRATO + VOLUME SLIDE
			tickEffects.Add(TickEffect7);               // tick effect 7. TREMOLO
			tickEffects.Add(/*TickEffect8*/NoEffect);   // tick effect 8. PAN
			tickEffects.Add(/*TickEffect9*/NoEffect);   // tick effect 9. SAMPLE OFFSET
			tickEffects.Add(TickEffectA);               // tick effect A. VOLUME SLIDE
			tickEffects.Add(/*TickEffectB*/NoEffect);   // tick effect B. POSITION JUMP
			tickEffects.Add(/*TickEffectC*/NoEffect);   // tick effect C. SET VOLUME
			tickEffects.Add(/*TickEffectD*/NoEffect);   // tick effect D. PATTERN BREAK
			tickEffects.Add(/*TickEffectE*/NoEffect);   // tick effect E. EXTEND EFFECTS
			tickEffects.Add(/*TickEffectF*/NoEffect);   // tick effect F. SET SPEED

			effectsE.Add(/*EffectE0*/NoEffect);         // E0. SET FILTER
			effectsE.Add(/*EffectE1*/NoEffect);         // E1. FINE PORTAMENTO UP
			effectsE.Add(/*EffectE2*/NoEffect);         // E2. FINE PORTAMENTO DOWN
			effectsE.Add(/*EffectE3*/NoEffect);         // E3. GLISSANDO CONTROL
			effectsE.Add(/*EffectE4*/NoEffect);         // E4. VIBRATO WAVEFORM 
			effectsE.Add(/*EffectE5*/NoEffect);         // E5. SET FINETUNE
			effectsE.Add(/*EffectE6*/NoEffect);         // E6. PATTERN LOOP
			effectsE.Add(/*EffectE7*/NoEffect);         // E7. TREMOLO WAVEFORM
			effectsE.Add(/*EffectE8*/NoEffect);         // E8. 16 POSITION PAN
			effectsE.Add(/*EffectE9*/NoEffect);         // E9. RETRIG NOTE
			effectsE.Add(EffectEA);                     // EA. FINE VOLUME SLIDE UP
			effectsE.Add(EffectEB);                     // EB. FINE VOLUME SLIDE DOWN
			effectsE.Add(/*EffectEC*/NoEffect);         // EC. CUT NOTE
			effectsE.Add(/*EffectED*/NoEffect);         // ED. NOTE DELAY
			effectsE.Add(/*EffectEE*/NoEffect);         // EE. PATTERN DELAY
			effectsE.Add(/*EffectEF*/NoEffect);         // EF. INVERT LOOP
		}

		public static uint GetNotePeriod(uint note, int fineTune)
		{
			float period = 7680.0f - note * 64.0f - fineTune * 0.5f;
			return (uint)period;
		}

		public static uint GetNoteFreq(uint note, int fineTune)
		{
			float period = GetNotePeriod(note, fineTune);
			float frequency = (float)(8363 * Math.Pow(2, (4608.0f - period) * 0.001302083f));
			return (uint)(ModuleConst.HALF_AMIGA_FREQUENCY / frequency);
		}

		public new float CalcPeriodIncrement(uint period)
		{
			return (period != 0) ? (ModuleConst.HALF_AMIGA_FREQUENCY / period) / ModuleConst.SOUNDFREQUENCY : 1.0f;
		}

		public float CalcClampPeriodIncrement(XM_MixerChannel mc)
		{
			mc.period = (mc.period < ModuleConst.MIN_PERIOD) ? ModuleConst.MIN_PERIOD : mc.period;
			mc.period = (mc.period > ModuleConst.MAX_PERIOD) ? ModuleConst.MAX_PERIOD : mc.period;
			//float frequency = (float)(8363 * Math.Pow(2, (4608.0f - mc.period) * 0.001302083f));
			return (mc.period != 0) ? (ModuleConst.HALF_AMIGA_FREQUENCY / mc.period) / ModuleConst.SOUNDFREQUENCY : 1.0f;
		}

		private void ResetChannelInstrument(XM_MixerChannel mc)
		{
            mc.instrumentPosition = 2;
            mc.instrumentLength = (int)mc.instrument.sample.sampleLength;
            mc.loopType = mc.instrument.sample.sampleLoopType;
            mc.instrumentLoopStart = mc.instrument.sample.sampleLoopStart;
            mc.instrumentLoopEnd = mc.instrument.sample.sampleLoopEnd;
            mc.currentFineTune = mc.instrument.sample.sampleFinetune;

            mc.vibratoCount = 0;
			mc.tremoloCount = 0;
			mc.arpeggioCount = 0;
		}
		
		private void UpdateNote()
		{
			patternDelay = 0;
			for (int ch = 0; ch < module.numberOfChannels; ch++)
			{
				XM_MixerChannel mc = mixChannels[ch];
				XM_PatternChannel pe = pattern.patternRows[(int)currentRow].patternChannels[ch];

				mc.patternChannel = pe;
				mc.effect = pe.effekt;
				mc.effectArg = pe.effektOp;
				mc.effectArgX = (mc.effectArg & 0xF0) >> 4;
				mc.effectArgY = (mc.effectArg & 0x0F);

				if (pe.noteIndex == 97)
                {
					mc.channelVolume = 0.0f;
                }

				if (pe.instrument > 0 && pe.noteIndex > 0)
				{
					mc.lastInstrument = mc.instrument;
					mc.instrument = module.instruments[(int)pe.instrument - 1];
					ResetChannelInstrument(mc);
					mc.channelVolume = mc.instrument.sample.sampleVolume;
					mc.portamentoStart = (int)mc.period;
					mc.noteIndex = (uint)(pe.noteIndex + mc.instrument.sample.relativeNoteNumber);
					mc.period = GetNoteFreq(mc.noteIndex, mc.currentFineTune);
					mc.portamentoEnd = (int)mc.period;
					mc.periodInc = CalcPeriodIncrement(mc.period);
				}

				if (pe.instrument > 0 && pe.noteIndex == 0 && module.instruments[(int)pe.instrument - 1] != mc.lastInstrument)
				{
					mc.lastInstrument = mc.instrument;
					mc.instrument = module.instruments[(int)pe.instrument - 1];
					ResetChannelInstrument(mc);
					mc.channelVolume = mc.instrument.sample.sampleVolume;
				}

				if (pe.instrument > 0 && pe.noteIndex == 0 && module.instruments[(int)pe.instrument - 1] == mc.lastInstrument)
					mc.channelVolume = mc.instrument.sample.sampleVolume;

				if (pe.instrument == 0 && pe.noteIndex > 0)
				{
					mc.portamentoStart = (int)mc.period;
					mc.noteIndex = (uint)(pe.noteIndex + mc.instrument.sample.relativeNoteNumber);
					mc.period = GetNoteFreq(mc.noteIndex, mc.currentFineTune);
					mc.portamentoEnd = (int)mc.period;
					mc.periodInc = CalcPeriodIncrement(mc.period);
					ResetChannelInstrument(mc);
				}
			}

			currentRow++;
			if (currentRow >= pattern?.patternRows.Count)
			{
				pattEnd = true;
				currentRow = 0;
				track++;
				if (track >= module.songLength)
				{
					moduleEnd = true;
					//if (!mixLoop) played = true;
					//else
					//{
					//    track = 0;
					//    pattern = module.patterns[module.arrangement[track]];
					//}
				}
				else pattern = module.patterns[(int)module.arrangement[(int)track]];
			}
		}

		private void UpdateVolumeEffects()
		{
			for (int ch = 0; ch < module.numberOfChannels; ch++)
			{
				byte volumeEffect = mixChannels[ch].volumeEffect;
				//if (volumeEffect >= 0x00 && volumeEffect <= 0x0F);  // do nothing
				if (volumeEffect >= 0x10 && volumeEffect <= 0x4F) mixChannels[ch].channelVolume = (float)(volumeEffect - 0x10) / 64.0f;  // v 0..63
				if (volumeEffect == 0x50) mixChannels[ch].channelVolume = 1.0f;  // v 64
				//if (volumeEffect >= 0x51 && volumeEffect <= 0x5F) return "...";  // undefined
				//if (volumeEffect >= 0x60 && volumeEffect <= 0x6F) return "D" + ModuleUtils.GetAsDec(volumeEffect - 0x60, 2);  // volumeEffect slide down
				//if (volumeEffect >= 0x70 && volumeEffect <= 0x7F) return "C" + ModuleUtils.GetAsDec(volumeEffect - 0x70, 2);  // volumeEffect slide up
				//if (volumeEffect >= 0x80 && volumeEffect <= 0x8F) return "B" + ModuleUtils.GetAsDec(volumeEffect - 0x80, 2);  // Fine volumeEffect down
				//if (volumeEffect >= 0x90 && volumeEffect <= 0x9F) return "A" + ModuleUtils.GetAsDec(volumeEffect - 0x90, 2);  // Fine volumeEffect up
				//if (volumeEffect >= 0xA0 && volumeEffect <= 0xAF) return "U" + ModuleUtils.GetAsDec(volumeEffect - 0xA0, 2);  // Vibrato speed
				//if (volumeEffect >= 0xB0 && volumeEffect <= 0xBF) return "H" + ModuleUtils.GetAsDec(volumeEffect - 0xB0, 2);  // Vibrato deph
				//if (volumeEffect >= 0xC0 && volumeEffect <= 0xCF) return "P" + ModuleUtils.GetAsDec((volumeEffect - 0xC0) * 4 + 2, 2);  // Set panning (2,6,10,14..62)
				//if (volumeEffect >= 0xD0 && volumeEffect <= 0xDF) return "L" + ModuleUtils.GetAsDec(volumeEffect - 0xD0, 2);  // Pan slide left
				//if (volumeEffect >= 0xE0 && volumeEffect <= 0xEF) return "R" + ModuleUtils.GetAsDec(volumeEffect - 0xE0, 2);  // Pan slide right
				//if (volumeEffect >= 0xF0 && volumeEffect <= 0xFF) return "G" + ModuleUtils.GetAsDec(volumeEffect - 0xF0, 2);  // Tone portamento
			}
		}


		private void UpdateNoteEffects()
		{
			for (int ch = 0; ch < module.numberOfChannels; ch++)
			{
				// TODO : XM effect may be more then 0x0F
				noteEffects[(int)(mixChannels[ch].effect & 0x0F)](mixChannels[ch]);
				if (mixChannels[ch].effect == 0 && mixChannels[ch].effectArg != 0) noteEffectsUsed[0] = true;
				if (mixChannels[ch].effect != 0) noteEffectsUsed[(int)(mixChannels[ch].effect & 0x0F)] = true;
			}
		}
		
		private void UpdateTickEffects()
		{
			for (int ch = 0; ch < module.numberOfChannels; ch++)
			{
				// TODO : XM effect may be more then 0x0F
				tickEffects[(int)(mixChannels[ch].effect & 0x0F)](mixChannels[ch]);
				if (mixChannels[ch].effect == 0 && mixChannels[ch].effectArg != 0) tickEffectsUsed[0] = true;
				if (mixChannels[ch].effect != 0) tickEffectsUsed[(int)(mixChannels[ch].effect & 0x0F)] = true;
			}
		}
		
		private void SetBPM()
		{
			mixerPosition = 0;
			samplesPerTick = CalcSamplesPerTick(BPM);
		}
		
		private void UpdateBPM()
		{
			if (counter >= ((1 + patternDelay) * speed))
			{
				counter = 0;
				UpdateNote();
				UpdateNoteEffects();
				UpdateTickEffects();
			}
			else UpdateTickEffects();
			counter++;
		}
		
		private void InitModule(uint startPosition = 0)
		{
			track = startPosition;
			counter = 0;
			patternDelay = 0;
			samplesPerTick = 0;
			mixerPosition = 0;
			currentRow = 0;
			pattern = module.patterns[(int)module.arrangement[(int)track]];
			BPM = module.BPM;
			speed = module.tempo;
			moduleEnd = false;

			mixChannels.Clear();
			for (int ch = 0; ch < module.numberOfChannels; ch++)
			{
				XM_MixerChannel mc = new XM_MixerChannel();
				mc.instrument = module.instruments[0];
				mc.lastInstrument = module.instruments[0];
				mixChannels.Add(mc);
			}
		}
		
		public void PlayModule(uint startPosition = 0)
		{
            //string mes = "Freq table : \n";
            //for (uint i = 0; i < 97; i++)
            //{
            //	mes += GetNotePeriod(i, 0) + ", ";
            //	if ((i & 0x0C) == 0) mes += "\n";
            //}
            //DebugMes(mes);

            //PlayInstrument(5);
            //return;

            playing = false;
			InitModule(startPosition);

			waveOut?.Stop();
			MixingTask?.Wait();

			waveReader?.Dispose();
			waveStream?.Dispose();
			waveOut?.Dispose();
			MixingTask?.Dispose();

			waveStream = new ModuleSoundStream(ModuleConst.SOUNDFREQUENCY, ModuleConst.MIX_CHANNELS == ModuleConst.STEREO);
			playing = true;
			MixData();

			MixingTask = Task.Factory.StartNew(() =>
			{
				while ((!moduleEnd) && playing)
				{
					MixData();
					while (waveStream.QueueLength > ModuleConst.MIX_WAIT && playing)
					{
						Thread.Sleep(100);
					}
					//DebugMes("Play position - " + waveOut.GetPosition() + " queue length - " + waveStream.QueueLength);
				}
				while (waveStream.QueueLength > 0 && playing)
				{
					Thread.Sleep(100);
					//DebugMes("Play position - " + waveOut.GetPosition() + " queue length - " + waveStream.QueueLength);
				}
			});

			waveReader = new WaveFileReader(waveStream);
			waveOut = new WaveOutEvent();
			waveOut.Init(waveReader);
			waveOut.Play();

		}

		public void PlayInstrument(int instrumentNumber = 0)
		{
			waveStream = new ModuleSoundStream(ModuleConst.SOUNDFREQUENCY, ModuleConst.MIX_CHANNELS == ModuleConst.STEREO);

			XM_MixerChannel ch = new XM_MixerChannel();
			ch.instrument = module.instruments[instrumentNumber - 1];
			XM_Sample sample = ch.instrument.sample;

			string mes = "\n\n\n";
			float periodInc = CalcPeriodIncrement(GetNotePeriod((uint)(48 + sample.relativeNoteNumber), 0));
			float pos = 0;
			while (pos < sample.sampleLength - 1)
			{
				short value = (short)(sample.sampleData[(int)pos] * ModuleConst.SOUND_AMP);
				waveStream.Write(value);
				if (((int)pos & 100) == 0) mes += value + ", ";
				pos += periodInc;
			}
			DebugMes(mes);

			waveReader = new WaveFileReader(waveStream);
			waveOut = new WaveOutEvent();
			waveOut.Init(waveReader);
			waveOut.Play();
		}


		public void Stop()
		{
			playing = false;
			waveOut.Stop();
		}
		
		private float GetSampleValueSimple(XM_MixerChannel mc)
		{
			return mc.instrument.sample.sampleData[(int)mc.instrumentPosition] * mc.channelVolume;
		}
		
		private float GetSampleValueLinear(XM_MixerChannel mc)
		{
			return 0;
			//int posInt = (int)mc.instrumentPosition;
			//float posReal = mc.instrumentPosition - posInt;
			//float a1 = mc.instrument.sample.sampleData[posInt];
			//if ((posInt + 1) >= mc.instrumentLength) return a1 * mc.channelVolume;
			//float a2 = mc.instrument.sample.sampleData[posInt + 1];
			//return (a1 + (a2 - a1) * posReal) * mc.channelVolume;
		}

		private float GetSampleValueSpline(XM_MixerChannel mc)
		{
			return 0;
			//int posInt = (int)mc.instrumentPosition;
			//float posReal = mc.instrumentPosition - posInt;
			//float a2 = mc.instrument.sample.sampleData[posInt];
			//if (((posInt - 1) < 0) || ((posInt + 2) >= mc.instrumentLength)) return a2 * mc.channelVolume;
			//float a1 = mc.instrument.sample.sampleData[posInt - 1];
			//float a3 = mc.instrument.sample.sampleData[posInt + 1];
			//float a4 = mc.instrument.sample.sampleData[posInt + 2];

			//float b0 = a1 + a2 + a2 + a2 + a2 + a3;
			//float b1 = -a1 + a3;
			//float b2 = a1 - a2 - a2 + a3;
			//float b3 = -a1 + a2 + a2 + a2 - a3 - a3 - a3 + a4;
			//return (((b3 * posReal * 0.1666666f + b2 * 0.5f) * posReal + b1 * 0.5f) * posReal + b0 * 0.1666666f) * mc.channelVolume;
		}

		private void MixData()
		{
			//var startTime = System.Diagnostics.Stopwatch.StartNew();
			//*
			string ms = "Mixed Channels " + module.numberOfChannels + " ";
			string mes = ms + " ";
			for (int pos = 0; pos < ModuleConst.MIX_LEN; pos++)
			{
				float mixValueR = 0;
				float mixValueL = 0;
				float mixValue = 0;
				for (int ch = 0; ch < module.numberOfChannels; ch++)
				{
					XM_MixerChannel mc = mixChannels[ch];
					//if (ch != 1) mc.muted = true;
					if (!mc.muted)
					{
						mc.FixInstrumentPosition();
						if ((mc.instrumentPosition >= 0) && (mc.instrumentPosition < mc.instrumentLength))
						{
							mixValue += GetSampleValueSimple(mc);
							//mixValue += GetSampleValueLinear(mc);
							//mixValue += GetSampleValueSpline(mc);
							if (ModuleConst.MIX_CHANNELS == ModuleConst.STEREO)
							{
								mixValueL += (((ch & 0x03) == 0) || ((ch & 0x03) == 3)) ? mixValue : 0;
								mixValueR += (((ch & 0x03) == 1) || ((ch & 0x03) == 2)) ? mixValue : 0;
							}
						}
						mc.instrumentPosition += (mc.periodInc * mc.instrumentDirection);
					}
				}
                if (ModuleConst.MIX_CHANNELS != ModuleConst.STEREO)
                    mixValue /= module.numberOfChannels;
                else
                {
                    mixValueL /= (module.numberOfChannels << 1);
                    mixValueR /= (module.numberOfChannels << 1);
                }

                if (ModuleConst.MIX_CHANNELS != ModuleConst.STEREO)
					waveStream.Write((short)(mixValue * ModuleConst.SOUND_AMP));
				else
				{
					waveStream.Write((short)((mixValueR * 0.75f + mixValueL * 0.25f) * ModuleConst.SOUND_AMP));
					waveStream.Write((short)((mixValueL * 0.75f + mixValueR * 0.25f) * ModuleConst.SOUND_AMP));
				}

				mixerPosition++;
				//if ((mixerPosition & 100) == 0) mes += (short)(mixValue * ModuleConst.SOUND_AMP) + ", ";
				if (mixerPosition >= samplesPerTick)
				{
					SetBPM();
					UpdateBPM();
				}
			}
			//DebugMes(mes + '\n');
			//*/
			//startTime.Stop();
			//var resultTime = startTime.Elapsed;
			//string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
			//                                    resultTime.Hours,
			//                                    resultTime.Minutes,
			//                                    resultTime.Seconds,
			//                                    resultTime.Milliseconds);
			//System.Diagnostics.Debug.WriteLine("Mixing time = " + elapsedTime);
		}
	
	} // end of class XM_Mixer

}
