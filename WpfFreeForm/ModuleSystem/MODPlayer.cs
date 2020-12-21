﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Media;

namespace ModuleSystem
{

	public class MODPattern : ModulePattern
	{
		private ModulePatternChannel createNewPatternChannel(Stream stream, int nSamples)
		{
			ModulePatternChannel channel = new ModulePatternChannel();
			int b0 = stream.ReadByte();
			int b1 = stream.ReadByte();
			int b2 = stream.ReadByte();
			int b3 = stream.ReadByte();

			channel.instrument = ((b0 & 0xF0) | ((b2 & 0xF0) >> 4)) & nSamples;
			channel.period = ((b0 & 0x0F) << 8) | b1;

			if (channel.period > 0) channel.noteIndex = (ModuleConst.getNoteIndexForPeriod(channel.period) + 1);

			channel.effekt = b2 & 0x0F;
			channel.effektOp = b3;

			return channel;
		}

		public void readPatternData(Stream stream, string modID, int nChannels, int nSamples)
		{
			if (modID == "FLT8") // StarTrekker is slightly different
			{
				for (int row = 0; row < 64; row++)
				{
					ModulePatternRow pRow = new ModulePatternRow();
					for (int channel = 0; channel < 4; channel++)
					{
						pRow.patternChannels.Add(createNewPatternChannel(stream, nSamples));
					}
					for (int channel = 4; channel < 8; channel++) 
						pRow.patternChannels.Add(new ModulePatternChannel());
					
					patternRows.Add(pRow);
				}
				for (int row = 0; row < 64; row++)
					for (int channel = 4; channel < 8; channel++)
						patternRows[row].patternChannels[channel] =	createNewPatternChannel(stream, nSamples);
			}
			else
			{
				for (int row = 0; row < 64; row++)
				{
					ModulePatternRow pRow = new ModulePatternRow();
					for (int channel = 0; channel < nChannels; channel++)
						pRow.patternChannels.Add(createNewPatternChannel(stream, nSamples));

					patternRows.Add(pRow);
				}
			}
		}
	}

	public class MODMixer : ModuleMixer
    {
		private bool tickEffect_00(ModuleMixerChannel mc)
		{
			System.Diagnostics.Debug.WriteLine("tickEffect -> 00");
			return true;
		}

		private bool tickEffect_01(ModuleMixerChannel mc)
		{
			System.Diagnostics.Debug.WriteLine("tickEffect -> 01");
			return true;
		}

		public MODMixer(SoundModule module) : base(module)
        {
			tickEffects.Add(tickEffect_00);
			tickEffects.Add(tickEffect_01);
		}
	}

	public class MODSoundModule : SoundModule
    {
		private MODMixer mixer		= null;

		public MODSoundModule():base("MOD format")
		{
			DebugMes("MOD Sound Module created");
			mixer = new MODMixer(this);
		}

		private long getAllInstrumentsLength()
		{
			long allSamplesLength = 0;
			foreach (ModuleInstrument inst in instruments) allSamplesLength += inst.length;
			return allSamplesLength;
		}

		public string InstrumentsToString()
		{
			string res = "Samples info : \n";
			int i = 0;
			foreach (ModuleInstrument inst in instruments)	res += ModuleUtils.getAsHex(i++, 2) + ':' + inst.ToString();
			return res;
		}

		private void readInstruments(Stream stream)
		{
			foreach (ModuleInstrument inst in instruments) inst.Clear();				
			instruments.Clear();
			for (int i = 0; i < nSamples; i++)
			{
				ModuleInstrument inst = new ModuleInstrument();
				instruments.Add(inst);
				inst.readInstrumentHeader(stream);
				DebugMes(inst.ToString());
			}
		}

		private void readArrangement(Stream stream)
		{
			songLength = stream.ReadByte(); // count of pattern in arrangement
			stream.ReadByte();              // skip, ood old CIAA

			arrangement.Clear();
			// always space for 128 pattern...
			for (int i = 0; i < 128; i++) arrangement.Add(stream.ReadByte());

		}

		private int calcPattCount()
		{
			int headerLen = 150; // Name+SongLen+CIAA+SongArrangement
			if (nSamples > 15) headerLen += 4;  // Kennung
						
			int sampleLen = 0;
			foreach (ModuleInstrument inst in instruments) sampleLen += 30 + inst.length;
			
			int spaceForPattern = (int)(fileLength - headerLen - sampleLen);
		
			// Lets find out about the highest Patternnumber used
			// in the song arrangement
			int maxPatternNumber = 0;
			for (int i = 0; i < songLength; i++)
			{
				int patternNumber = arrangement[i];
				if (patternNumber > maxPatternNumber && patternNumber < 0x80)
					maxPatternNumber = arrangement[i];
			}
			maxPatternNumber++; // Highest number becomes highest count 

			// It could be the WOW-Format:
			if (modID == "M.K.")
			{
				// so check for 8 channels:
				int totalPatternBytes = maxPatternNumber * 2048; //64*4*8
				// This mod has 8 channels! --> WOW
				if (totalPatternBytes == spaceForPattern)
				{
					isAmigaLike = true;
					nChannels = 8;
					trackerName = "Grave Composer";
				}
			}

			int bytesPerPattern = 256 * nChannels; //64*4*nChannels
			nPatterns = (int)(spaceForPattern / bytesPerPattern);
			int bytesLeft = (int)(spaceForPattern % bytesPerPattern);

			if (bytesLeft > 0) // It does not fit!
			{
				if (maxPatternNumber > nPatterns)
				{
					// The modfile is too short. The highest pattern is reaching into
					// the sampledata, but it has to be read!
					bytesLeft -= bytesPerPattern;
					nPatterns = maxPatternNumber + 1;
				}
				else
				{
					// The modfile is too long. Sometimes this happens if composer
					// add additional data to the modfile.
					bytesLeft += (nPatterns - maxPatternNumber) * bytesPerPattern;
					nPatterns = maxPatternNumber;
				}
				return bytesLeft;
			}
			
			return 0;
		}

		private void readPatterns(Stream stream)
		{
			foreach (MODPattern pat in patterns) pat.Clear();
			patterns.Clear();
			if (nSamples > 15) stream.Seek(4, SeekOrigin.Current);	// skip ModID, if not NoiseTracker:
																	
			bytesLeft = calcPattCount();
            for (int i = 0; i < nPatterns; i++)						// Read the patterndata
			{
                MODPattern pattern = new MODPattern();
                pattern.readPatternData(stream, modID, nChannels, nSamples);
                patterns.Add(pattern);
				//DebugMes("Patern : " + i + pattern.ToString());
            }
        }

		private void readInstrumentsData(Stream stream)
		{
			// Sampledata: If the modfile was too short, we need to recalculate:
			if (bytesLeft < 0)
			{
				long calcSamplePos = getAllInstrumentsLength();
				calcSamplePos = fileLength - calcSamplePos;
				// do this only, if needed!
				if (calcSamplePos < stream.Position) stream.Seek(calcSamplePos, SeekOrigin.Begin);
			}
			for (int i = 0; i < nSamples; i++) instruments[i].readInstrumentData(stream);
		}

		public override bool readFromStream(Stream stream)
		{
			fileLength = stream.Length;
			baseVolume = 128;
			BPMSpeed = 125;
			tempo = 6;

			if (!checkFormat(stream)) return false;
			setModType();

			stream.Seek(0, SeekOrigin.Begin);
			songName = ModuleUtils.readString0(stream, 20);
			nInstruments = nSamples;
			readInstruments(stream);		// read instruments		
			readArrangement(stream);		// read pattern order	
			readPatterns(stream);			// read patterns
			readInstrumentsData(stream);	// read samples data

			//mixer = new cMODMixer(this);
			//mixer.mixInfo.BPM = BPMSpeed;
			//mixer.mixInfo.speed = tempo;

			return true;
		}

		private void setModType()
		{
			isAmigaLike = false;
			nSamples = 31;
			trackerName = "StarTrekker";

			if (modID == "M.K." || modID == "M!K!" || modID == "M&K!" || modID == "N.T.")
			{
				isAmigaLike = true; 
				nChannels = 4;
				trackerName = "ProTracker";
			}
			
			if (modID.Substring(0, 3) == "FLT" || modID.Substring(0, 3) == "TDZ")
				nChannels = int.Parse(modID.Substring(3, 1));

			if (modID.Substring(1, 3) == "CHN")
				nChannels = int.Parse(modID.Substring(0, 1));

			if (modID == "CD81" || modID == "OKTA")
			{
				nChannels = 8;
				trackerName = "Atari Oktalyzer";
			}

			if (modID.Substring(2, 2) == "CH" || modID.Substring(2, 2) == "CN")
			{
				nChannels = int.Parse(modID.Substring(0, 2));
				trackerName = "TakeTracker";
			}	 
		}

		public override bool checkFormat(Stream stream)
		{
			byte[] bytesID = new byte[4];
			stream.Seek(1080, SeekOrigin.Begin);
			stream.Read(bytesID);
			modID = Encoding.ASCII.GetString(bytesID);
            return modID == "M.K." ||
                   modID == "M!K!" ||
                   modID == "M&K!" ||
                   modID == "N.T." ||
                   modID == "CD81" ||
                   modID == "OKTA" ||
                   modID.Substring(0, 3) == "FLT" ||
                   modID.Substring(0, 3) == "TDZ" ||
                   modID.Substring(1, 3) == "CHN" ||
                   modID.Substring(2, 2) == "CH"  ||
				   modID.Substring(2, 2) == "CN";
		}

		public override string ToString()
		{
			string modInfo = "";
			modInfo += "Mod Length " + this.fileLength + "\n";
			modInfo += "Mod with " + nSamples + " samples and " + nChannels + " channels using\n";
			modInfo += "Tracker : " + trackerName + " modID : " + modID + "\n";
			modInfo += (isAmigaLike) ? "Protracker" : "Fast Tracker log";
			modInfo += " frequency table\n";
			modInfo += "Song named : " + songName + "\n";
			modInfo += "SongLength : " + songLength + "\n";

			for (int i = 0; i < songLength; i++) modInfo += arrangement[i] + ((i < songLength - 1) ? "," : "");

			modInfo += "\n\n";
			return modInfo;
		}

		public override void PlayInstrument(int num)
        {
			if (num < 0 || num >= nSamples) return;

			soundSystem.Stop();
			//DebugMes("Instruments count - " + instruments.Count);
			ModuleInstrument inst = instruments[num];
			
			uint samplesPerSecond = 44100;
			float baseFreq = /*3546895.0f / */(float)(inst.baseFrequency * 2);
			float frqMul = baseFreq / (float)(samplesPerSecond);
			DebugMes("BaseFreq = " + inst.baseFrequency + " Freq mull = " + frqMul);
			uint soundBufferLen = (uint)(inst.length / frqMul); 
			DebugMes("SoundBufferLen = " + soundBufferLen + " Instrument len = " + inst.length);

			BinaryWriter buffer = soundSystem.getBuffer;
			soundSystem.SetBufferLen(soundBufferLen);
			float fpos = 0;
			for (int i = 0; i < soundBufferLen; i++)
			{
				if (fpos < inst.length) buffer.Write((short)(32767 * inst.instrumentData[(int)(fpos)]));
				fpos += frqMul;
			}
			soundSystem.Play();
		}
	}
}
