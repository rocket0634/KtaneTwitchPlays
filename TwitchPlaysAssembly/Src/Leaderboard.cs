﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

public class Leaderboard
{
	public class LeaderboardEntry
	{
		public string UserName
		{
			get;
			set;
		}

		public Color UserColor
		{
			get;
			set;
		}

		public int SolveCount
		{
			get;
			set;
		}

		public int StrikeCount
		{
			get;
			set;
		}

		public int SolveScore
		{
			get;
			set;
		}

		public DateTime LastAction
		{
			get;
			set;
		}

		public int Rank
		{
			get;
			set;
		}

		public float RecordSoloTime
		{
			get;
			set;
		}

		public float TotalSoloTime
		{
			get;
			set;
		}

		public int TotalSoloClears
		{
			get;
			set;
		}

		public int SoloRank
		{
			get;
			set;
		}

		[JsonConverter(typeof(StringEnumConverter))]
		public OtherModes.Team Team { get; set; } = OtherModes.Team.Good;
		
		public int Level
		{
			get;
			set;
		}

		public double EXPToNextLevel
		{
			get;
			set;
		}

		public int MaxHP
		{
			get;
			set;
		}

		public int CurrentHP
		{
			get;
			set;
		}

		public int Gold
		{
			get;
			set;
		}

		public int Prestige
		{
			get;
			set;
		}

		public int ClericLV
		{
			get;
			set;
		}

		public int DefenderLV
		{
			get;
			set;
		}

		public int TricksterLV
		{
			get;
			set;
		}

		public int WizardLV
		{
			get;
			set;
		}

		public string CurrentClass
		{
			get;
			set;
		}

		public string SecondaryClass
		{
			get;
			set;
		}

		public DateTime LastUsedSpell
		{
			get;
			set;
		}

		public float TimePerSoloSolve
		{
			get
			{
				if (TotalSoloClears == 0)
				{
					return 0;
				}
				return RecordSoloTime / RequiredSoloSolves;
			}
		}

		public void AddSolve(int num)
		{
			SolveCount += num;
		}

		public void AddStrike(int num)
		{
			StrikeCount += num;
		}

		public void AddScore(int num)
		{
			SolveScore += num;
		}

		public void AddCoin(int num)
		{
			Gold += num;
		}

		public void AddPrestige(int num)
		{
			Prestige += num;
		}


	}

	public bool CheckFunds(string userName, int cost)
	{
		LeaderboardEntry entry = GetEntry(userName);
		if (entry.Gold >= cost)
		{
			entry.Gold -= cost;
			return true;
		}
		else
			return false;
	}


	private Color SafeGetColor(string userName)
	{
		return IRCConnection.GetUserColor(userName);
	}

	private bool GetEntry(string UserName, out LeaderboardEntry entry) => _entryDictionary.TryGetValue(UserName.ToLowerInvariant(), out entry);

	public void SecondaryClass(string UserName, string Class2)
	{
		LeaderboardEntry entry = GetEntry(UserName);
		if ((Class2 == "cleric") || (Class2 == "defender") || (Class2 == "wizard") || (Class2 == "trickster"))
		{
			TextInfo temp = new CultureInfo("en-US", false).TextInfo;
			entry.SecondaryClass = Class2;
			IRCConnection.SendMessageFormat("Secondary class has been set to {0}", temp.ToTitleCase(Class2));
		}
		else
		{
			IRCConnection.SendMessage("Invalid class choice.");
		}

	}

	public LeaderboardEntry LevelUp(string UserName, string NewClass)
	{
		LeaderboardEntry entry = GetEntry(UserName);
		if (entry.EXPToNextLevel != 0)
		{
			IRCConnection.SendMessage("Sorry, you have insufficient experience to level up currently.");
		}
		else
		{

			switch (NewClass)
			{
				case "cleric":
					if (entry.ClericLV == 5)
					{
						IRCConnection.SendMessage("Sorry, you have maxed out that class.");
						return entry;
					}
					entry.Level += 1;
					entry.CurrentHP += 3;
					entry.MaxHP += 3;
					entry.ClericLV += 1;
					entry.CurrentClass = "Cleric";
					IRCConnection.SendMessageFormat("Congratuations, {0}, you are now a LV {1} Cleric.", UserName, entry.ClericLV);
					ResetEXPNeeded(UserName);
					return entry;
				case "defender":
					if (entry.DefenderLV == 5)
					{
						IRCConnection.SendMessage("Sorry, you have maxed out that class.");
						return entry;
					}
					entry.Level += 1;
					entry.CurrentHP += 6;
					entry.MaxHP += 6;
					entry.DefenderLV += 1;
					entry.CurrentClass = "Defender";
					IRCConnection.SendMessageFormat("Congratuations, {0}, you are now a LV {1} Defender.", UserName, entry.DefenderLV);
					ResetEXPNeeded(UserName);
					return entry;
				case "trickster":
					if (entry.TricksterLV == 5)
					{
						IRCConnection.SendMessage("Sorry, you have maxed out that class.");
						return entry;
					}
					entry.Level += 1;
					System.Random rnd = new System.Random();
					int temp = rnd.Next(3, 7);
					entry.CurrentHP += temp;
					entry.MaxHP += temp;
					entry.TricksterLV += 1;
					entry.CurrentClass = "Trickster";
					IRCConnection.SendMessageFormat("Congratuations, {0}, you are now a LV {1} Trickster.", UserName, entry.TricksterLV);
					ResetEXPNeeded(UserName);
					return entry;
				case "wizard":
					if (entry.WizardLV == 5)
					{
						IRCConnection.SendMessage("Sorry, you have maxed out that class.");
						return entry;
					}
					entry.Level += 1;
					entry.CurrentHP += 4;
					entry.MaxHP += 4;
					entry.WizardLV += 1;
					entry.CurrentClass = "Wizard";
					IRCConnection.SendMessageFormat("Congratuations, {0}, you are now a LV {1} Wizard.", UserName, entry.WizardLV);
					ResetEXPNeeded(UserName);
					return entry;
				default:
					IRCConnection.SendMessageFormat("Sorry, unable to find a class named {0}", NewClass);
					return entry;
			}
		}
		return entry;
	}

	public void ResetEXPNeeded(string userName)
	{
		LeaderboardEntry entry = GetEntry(userName);
		entry.EXPToNextLevel = entry.Level * 250;
	}

	private LeaderboardEntry GetEntry(string userName)
	{
		if (!GetEntry(userName, out LeaderboardEntry entry))
		{
			entry = new LeaderboardEntry();
			_entryDictionary[userName.ToLowerInvariant()] = entry;
			_entryList.Add(entry);
			entry.UserColor = SafeGetColor(userName);
			entry.EXPToNextLevel = 100;
			entry.MaxHP = 5;
			entry.CurrentHP = 5;
			entry.CurrentClass = "Human";
			entry.SecondaryClass = "Human";
		}
		entry.UserName = userName;
		return entry;
	}

	private LeaderboardEntry GetEntry(string userName, Color userColor)
	{
		LeaderboardEntry entry = GetEntry(userName);
		entry.UserName = userName;
		entry.UserColor = userColor;
		return entry;
	}

	public LeaderboardEntry AddSoloClear(string userName, float newRecord, out float previousRecord)
	{
		LeaderboardEntry entry = _entryDictionary[userName.ToLowerInvariant()];
		previousRecord = entry.RecordSoloTime;
		if ((entry.TotalSoloClears < 1) || (newRecord < previousRecord))
		{
			entry.RecordSoloTime = newRecord;
		}
		entry.TotalSoloClears++;
		entry.TotalSoloTime += newRecord;
		ResetSortFlag();

		if (entry.TotalSoloClears == 1)
		{
			_entryListSolo.Add(entry);
		}

		SoloSolver = entry;
		return entry;
	}

	public void AddSolve(string userName, int NumSolve = 1)
	{
		LeaderboardEntry entry = GetEntry(userName, SafeGetColor(userName));

		entry.AddSolve(NumSolve);
		entry.LastAction = DateTime.Now;
		ResetSortFlag();

		string name = userName.ToLowerInvariant();
		CurrentSolvers[name] = CurrentSolvers.TryGetValue(name, out int value) ? value + 1 : 1;
	}

	public void AddSolves(string userName)
	{
		LeaderboardEntry entry = GetEntry(userName, SafeGetColor(userName));

		entry.AddSolve(1);
		entry.LastAction = DateTime.Now;
		ResetSortFlag();

		string name = userName.ToLowerInvariant();
		CurrentSolvers[name] = CurrentSolvers.TryGetValue(name, out int value) ? value + 1 : 1;
	}

	public void AddSolves(string userName, Color userColor, int num = 1)
	{
		LeaderboardEntry entry = GetEntry(userName, userColor);

		entry.AddSolve(num);
		entry.LastAction = DateTime.Now;
		ResetSortFlag();

		string name = userName.ToLowerInvariant();
		CurrentSolvers[name] = CurrentSolvers.TryGetValue(name, out int value) ? value + 1 : 1;
	}
	public void AddStrike(string userName, int numStrikes = 1) => AddStrike(userName, SafeGetColor(userName), numStrikes);

	public void AddStrike(string userName, Color userColor, int numStrikes = 1)
	{
		LeaderboardEntry entry = GetEntry(userName, userColor);

		entry.AddStrike(numStrikes);
		entry.LastAction = DateTime.Now;
		ResetSortFlag();
	}

	public void AddScore(string userName, int numScore) => AddScore(userName, SafeGetColor(userName), numScore);

	public void AddPP(string userName, int PP)
	{
		AddCoin(userName, SafeGetColor(userName), PP);
		AddPrestige(userName, SafeGetColor(userName), PP);
	}

	public void AddCoin(string userName, Color userColor, int NumScore)
	{
		LeaderboardEntry entry = GetEntry(userName, userColor);
		entry.AddCoin(NumScore);
	}

	public void AddPrestige(string userName, Color userColor, int NumScore)
	{
		LeaderboardEntry entry = GetEntry(userName, userColor);
		entry.AddPrestige(NumScore);
	}

	public void AddScore(string userName, Color userColor, int numScore)
	{
		LeaderboardEntry entry = GetEntry(userName, userColor);
		entry.AddScore(numScore);
		if (entry.EXPToNextLevel > 0)
		{
			double EXPmultiplier = 1.0;
			if (numScore > 0) entry.EXPToNextLevel -= (numScore * EXPmultiplier);
			if (entry.EXPToNextLevel <= 0)
			{
				IRCConnection.SendMessageFormat("{0}, you have attained enough experience to gain a level!  Use !levelup <class> to level up, and choose your active class, and/or !class2 <class> to choose your secondary.", userName);
				entry.EXPToNextLevel = 0;
			}
		}
		entry.LastAction = DateTime.Now;
		ResetSortFlag();
	}

	public void MakeEvil(string userName) => MakeEvil(userName, SafeGetColor(userName));

	public void MakeEvil(string userName, Color userColor)
	{
		LeaderboardEntry entry = GetEntry(userName, userColor);
		entry.Team = OtherModes.Team.Evil;
		entry.LastAction = DateTime.Now;
	}

	public void MakeGood(string userName) => MakeGood(userName, SafeGetColor(userName));

	public void MakeGood(string userName, Color userColor)
	{
		LeaderboardEntry entry = GetEntry(userName, userColor);
		entry.Team = OtherModes.Team.Good;
		entry.LastAction = DateTime.Now;
	}

	public bool isAnyEvil() => _entryList.Any(x => x.Team == OtherModes.Team.Evil);

	public bool isAnyGood() => _entryList.Any(x => x.Team == OtherModes.Team.Good);

	public OtherModes.Team GetTeam(string userName) => GetTeam(userName, SafeGetColor(userName));

	public OtherModes.Team GetTeam(string userName, Color userColor)
	{
		LeaderboardEntry entry = GetEntry(userName, userColor);
		return entry.Team;
	}

	public float GetStrikeMultiMulti(string userName)
	{
		float multi = 1.5f;
		LeaderboardEntry entry = GetEntry(userName);
		if ((entry.CurrentClass == "Cleric") || (entry.CurrentClass == "cleric"))
		{
			multi *= 2;
		}
		return multi;
	}

	public int GetClericTimeBoost(string userName)
	{
		LeaderboardEntry entry = GetEntry(userName);
		if ((entry.CurrentClass == "Cleric") || (entry.CurrentClass == "cleric") || (entry.SecondaryClass == "Cleric") || (entry.SecondaryClass == "cleric"))
		{
			switch (entry.ClericLV)
			{
				case 0:
					return 0;
				case 1:
					return 10;
				case 2:
				case 3:
					return 20;
				case 4:
					return 30;
				case 5:
				case 6:
					return 40;
			}
		}
		return 0;
	}

	public float GetClericMultiBoost(string userName)
	{
		float multi = 1.0f;
		LeaderboardEntry entry = GetEntry(userName);
		if ((entry.CurrentClass == "Cleric") || (entry.CurrentClass == "cleric") || (entry.SecondaryClass == "Cleric") || (entry.SecondaryClass == "cleric"))
		{
			switch (entry.ClericLV)
			{
				case 1:
					multi = 2.0f;
					break;
				case 2:
				case 3:
					multi = 3.0f;
					break;
				case 4:
					multi = 4.0f;
					break;
				case 5:
				case 6:
					multi = 5.0f;
					break;
			}
		}
		if ((entry.CurrentClass == "Wizard") || (entry.CurrentClass == "wizard")) multi *= .75f;
		return multi;
	}
	
	public float WizardFreezeTimeBonus(string userName, int Original)
	{
		LeaderboardEntry entry = GetEntry(userName);
		if ((entry.CurrentClass == "Wizard") || (entry.CurrentClass == "wizard") || (entry.SecondaryClass == "Wizard") || (entry.SecondaryClass == "wizard"))
		{
			switch (entry.WizardLV)
			{
				case 1:
				case 2:
					IRCConnection.SendMessageFormat("Wizard {0} has frozen time for {1} seconds.", userName, Original);
					return Original;
				case 3:
				case 4:
					IRCConnection.SendMessageFormat("Wizard {0} has frozen time for {1} seconds.", userName, Original*2);
					return Original * 2;
				case 5:
				case 6:
					IRCConnection.SendMessageFormat("Wizard {0} has frozen time for {1} seconds.", userName, Original*3);
					return Original * 3;
				default:
					return 0;
			}
		}
		return 0;
	}


	public float WizardTimeBonus(string userName, int Original)
	{
		float bonus = Original;
		LeaderboardEntry entry = GetEntry(userName);
		if ((entry.CurrentClass == "Wizard") || (entry.CurrentClass == "wizard") || (entry.SecondaryClass == "Wizard") || (entry.SecondaryClass == "wizard"))
		{
			switch (entry.WizardLV)
			{
				case 1:
				case 2:
					return Original;
				case 3:
				case 4:
					return Original*2;
				case 5:
				case 6:
					return Original * 3;
				default:
					return 0;
			}
		}
		return 0;
	}

	public void SpellCast(string userName)
	{
		LeaderboardEntry entry = GetEntry(userName);
		entry.LastUsedSpell = DateTime.Now;
	}

	public bool CheckChaos(string userName)
	{
		LeaderboardEntry entry = GetEntry(userName);
		if ((entry.CurrentClass == "Trickster") || (entry.CurrentClass == "trickster") || (entry.SecondaryClass == "Trickster") || (entry.SecondaryClass == "trickster"))
		{
			if (entry.TricksterLV < 3)
			{
				IRCConnection.SendMessage("You are not high enough level to cast this spell.");
				return false;
			}
			if ((DateTime.Now - entry.LastUsedSpell).TotalHours > 24)
			{
				return true;
			}
			IRCConnection.SendMessage("You cannot cast a spell again this quickly.  You must wait 24 hours between spell casts.");

		}
		return false;
	}

    public bool CheckHeal(string userName, int level)
    {
        LeaderboardEntry entry = GetEntry(userName);
        if ((entry.CurrentClass.ToLowerInvariant() == "cleric")  || (entry.SecondaryClass.ToLowerInvariant() == "cleric"))
        {
            if (entry.ClericLV < level)
            {
                IRCConnection.SendMessage("You are not high enough level to cast this spell.");
                return false;
            }
            if ((DateTime.Now - entry.LastUsedSpell).TotalHours > 24)
            {
                return true;
            }
            IRCConnection.SendMessage("You cannot cast a spell again this quickly.  You must wait 24 hours between spell casts.");

        }
        return false;
    }

    public bool NoClaims(string userName)
	{
		LeaderboardEntry entry = GetEntry(userName);
		if ((entry.CurrentClass == "Trickster") || (entry.CurrentClass == "trickster")) return true;
		return false;
	}

	public float EXPMultiplier(string userName)
	{
		float multi = 1.0f;
		LeaderboardEntry entry = GetEntry(userName);
		if ((entry.CurrentClass == "Trickster") || (entry.CurrentClass == "trickster") || (entry.SecondaryClass == "Trickster") || (entry.SecondaryClass == "trickster"))
		{
			switch (entry.TricksterLV)
			{
				case 1:
					multi *= 1.1f;
					break;
				case 2:
				case 3:
					multi *= 1.2f;
					break;
				case 4:
					multi *= 1.3f;
					break;
				case 5:
					multi *= 1.4f;
					break;
				case 6:
					multi *= 1.5f;
					break;
				default:
					break;
			}
		}
		return multi;
	}

	public IEnumerable<LeaderboardEntry> GetSortedEntries(int count)
	{
		CheckAndSort();
		return _entryList.Take(count);
	}

	public IEnumerable<LeaderboardEntry> GetSortedSoloEntries(int count)
	{
		CheckAndSort();
		return _entryListSolo.Take(count);
	}

	public IEnumerable<LeaderboardEntry> GetSortedEntriesIncluding(Dictionary<string, int>.KeyCollection extras, int count)
	{
		var entries = new List<LeaderboardEntry>();

		foreach (string name in extras)
			if (GetEntry(name, out LeaderboardEntry entry))
				entries.Add(entry);

		if (entries.Count < count)
		{
			entries.AddRange(GetSortedEntries(count).Except(entries).Take(count - entries.Count));
		}

		entries.Sort(CompareScores);
		return entries.Take(count);
	}

	public IEnumerable<LeaderboardEntry> GetSortedSoloEntriesIncluding(string userName, int count)
	{
		List<LeaderboardEntry> ranking = GetSortedSoloEntries(count).ToList();

		LeaderboardEntry entry = _entryDictionary[userName.ToLowerInvariant()];
		if (entry.SoloRank > count)
		{
			ranking.RemoveAt(ranking.Count - 1);
			ranking.Add(entry);
		}

		return ranking;
	}

	public int GetRank(string userName, out LeaderboardEntry entry)
	{
		if (!GetEntry(userName, out entry))
		{
			return _entryList.Count + 1;
		}

		CheckAndSort();
		return _entryList.IndexOf(entry) + 1;
	}

	public void GetStatus(string userName)
	{
		LeaderboardEntry entry = GetEntry(userName);
		IRCConnection.SendMessageFormat("{0} (HP: {11}/{12}) is a Level {1} {2}/{6}, with {3} gold and {4} prestige.  {5} EXP required to level up.  Class levels:  C: {7} D: {8} T: {9} W: {10}", userName, entry.Level, entry.CurrentClass, entry.Gold, entry.Prestige, entry.EXPToNextLevel, entry.SecondaryClass, entry.ClericLV, entry.DefenderLV, entry.TricksterLV, entry.WizardLV, entry.CurrentHP, entry.MaxHP);
	}

	public int GetRank(int rank, out LeaderboardEntry entry)
	{
		CheckAndSort();
		entry = (_entryList.Count >= rank) ? _entryList[rank - 1] : null;
		return entry?.Rank ?? 0;
	}

	public int GetSoloRank(int rank, out LeaderboardEntry entry)
	{
		CheckAndSort();
		entry = (_entryListSolo.Count >= rank) ? _entryListSolo[rank - 1] : null;
		return entry?.SoloRank ?? 0;
	}

	public int GetSoloRank(string userName, out LeaderboardEntry entry)
	{
		entry = _entryListSolo.FirstOrDefault(x => string.Equals(x.UserName, userName, StringComparison.InvariantCultureIgnoreCase));
		if (entry != null)
			return _entryListSolo.IndexOf(entry);
		else
			return 0;
	}

	public bool IsDuplicate(LeaderboardEntry person, out List<LeaderboardEntry> entries)
	{
		if (_entryDictionary.ContainsValue(person))
		{
			entries = _entryList.Where(x => x.SolveScore == person.SolveScore && x.UserName != person.UserName).ToList();
			if (entries != null && entries.Any())
				return true;
			else
			{
				entries = null;
				return false;
			}
		}
		entries = null;
		return false;
	}

	public bool IsSoloDuplicate(LeaderboardEntry person, out List<LeaderboardEntry> entries)
	{
		if (_entryListSolo.Contains(person))
		{
			entries = _entryListSolo.Where(x => x.RecordSoloTime == person.RecordSoloTime && x.UserName != person.UserName).ToList();
			if (entries != null && entries.Any())
				return true;
			else
			{
				entries = null;
				return false;
			}
		}
		entries = null;
		return false;
	}

	public void GetTotalSolveStrikeCounts(out int solveCount, out int strikeCount, out int scoreCount)
	{
		solveCount = 0;
		strikeCount = 0;
		scoreCount = 0;

		foreach (LeaderboardEntry entry in _entryList)
		{
			solveCount += entry.SolveCount;
			strikeCount += entry.StrikeCount;
			scoreCount += entry.SolveScore;
		}
	}

	public void AddEntry(LeaderboardEntry user)
	{
		LeaderboardEntry entry = GetEntry(user.UserName, user.UserColor);
		entry.SolveCount = user.SolveCount;
		entry.StrikeCount = user.StrikeCount;
		entry.SolveScore = user.SolveScore;
		entry.LastAction = user.LastAction;
		entry.RecordSoloTime = user.RecordSoloTime;
		entry.Team = user.Team;
		entry.TotalSoloTime = user.TotalSoloTime;
		entry.TotalSoloClears = user.TotalSoloClears;
		entry.Level = user.Level;
		entry.EXPToNextLevel = user.EXPToNextLevel;
		entry.MaxHP = user.MaxHP;
		entry.CurrentHP = user.CurrentHP;
		entry.Gold = user.Gold;
		entry.Prestige = user.Prestige;
		entry.ClericLV = user.ClericLV;
		entry.DefenderLV = user.DefenderLV;
		entry.TricksterLV = user.TricksterLV;
		entry.WizardLV = user.WizardLV;
		entry.CurrentClass = user.CurrentClass;
		entry.SecondaryClass = user.SecondaryClass;
		entry.LastUsedSpell = user.LastUsedSpell;

		if (entry.TotalSoloClears > 0)
		{
			_entryListSolo.Add(entry);
		}
	}

	public void AddEntries(List<LeaderboardEntry> entries)
	{
		foreach (LeaderboardEntry entry in entries)
		{
			AddEntry(entry);
		}
	}

	public void DeleteEntry(LeaderboardEntry user)
	{
		_entryDictionary.Remove(user.UserName.ToLowerInvariant());
		_entryList.Remove(user);
	}

	public void DeleteEntry(string userNickName) => DeleteEntry(GetEntry(userNickName));

	public void DeleteSoloEntry(LeaderboardEntry user) => _entryListSolo.Remove(user);

	public void DeleteSoloEntry(string userNickName) => DeleteSoloEntry(_entryListSolo.First(x => string.Equals(x.UserName, userNickName, StringComparison.InvariantCultureIgnoreCase)));

	public void ResetLeaderboard()
	{
		_entryDictionary.Clear();
		_entryList.Clear();
		_entryListSolo.Clear();
		CurrentSolvers.Clear();
		BombsAttempted = 0;
		BombsCleared = 0;
		BombsExploded = 0;
		OldBombsAttempted = 0;
		OldBombsCleared = 0;
		OldBombsExploded = 0;
		OldScore = 0;
		OldSolves = 0;
		OldStrikes = 0;
	}

	private void ResetSortFlag() => _sorted = false;

	private void CheckAndSort()
	{
		if (!_sorted)
		{
			_entryList.Sort(CompareScores);
			_entryListSolo.Sort(CompareSoloTimes);
			_sorted = true;

			int i = 1;
			LeaderboardEntry previous = null;
			foreach (LeaderboardEntry entry in _entryList)
			{
				if (previous == null)
				{
					entry.Rank = 1;
				}
				else
				{
					entry.Rank = (CompareScores(entry, previous) == 0) ? previous.Rank : i;
				}
				previous = entry;
				i++;
			}

			i = 1;
			foreach (LeaderboardEntry entry in _entryListSolo)
			{
				entry.SoloRank = i++;
			}
		}
	}

	private static int CompareScores(LeaderboardEntry lhs, LeaderboardEntry rhs)
	{
		if (lhs.SolveScore != rhs.SolveScore)
		{
			//Intentially reversed comparison to sort from highest to lowest
			return rhs.SolveScore.CompareTo(lhs.SolveScore);
		}

		//Intentially reversed comparison to sort from highest to lowest
		return rhs.SolveScore.CompareTo(lhs.SolveScore);
	}

	private static int CompareSoloTimes(LeaderboardEntry lhs, LeaderboardEntry rhs) => lhs.RecordSoloTime.CompareTo(rhs.RecordSoloTime);

	public void ClearSolo()
	{
		SoloSolver = null;
		CurrentSolvers.Clear();
	}

	public void LoadDataFromFile()
	{
		string path = Path.Combine(Application.persistentDataPath, usersSavePath);
		try
		{
			DebugHelper.Log($"Leaderboard: Loading leaderboard data from file: {path}");
			XmlSerializer xml = new XmlSerializer(_entryList.GetType());
			TextReader reader = new StreamReader(path);
			List<LeaderboardEntry> entries = (List<LeaderboardEntry>)xml.Deserialize(reader);
			AddEntries(entries);
			ResetSortFlag();

			path = Path.Combine(Application.persistentDataPath, statsSavePath);
			DebugHelper.Log($"Leaderboard: Loading stats data from file: {path}");
			string jsonInput = File.ReadAllText(path);
			Dictionary<string, int> stats = JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonInput);

			BombsAttempted = stats["BombsAttempted"];
			BombsCleared = stats["BombsCleared"];
			BombsExploded = stats["BombsExploded"];
			OldBombsAttempted = BombsAttempted;
			OldBombsCleared = BombsCleared;
			OldBombsExploded = BombsExploded;

			GetTotalSolveStrikeCounts(out OldSolves, out OldStrikes, out OldScore);
		}
		catch (FileNotFoundException)
		{
			DebugHelper.LogWarning($"Leaderboard: File {path} was not found.");
		}
		catch (Exception ex)
		{
			DebugHelper.LogException(ex);
		}
	}

	public void SaveDataToFile()
	{
		string path = Path.Combine(Application.persistentDataPath, usersSavePath);
		try
		{
			if (!_sorted)
			{
				CheckAndSort();
			}

			DebugHelper.Log($"Leaderboard: Saving leaderboard data to file: {path}");
			XmlSerializer xml = new XmlSerializer(_entryList.GetType());
			TextWriter writer = new StreamWriter(path);
			xml.Serialize(writer, _entryList);

			path = Path.Combine(Application.persistentDataPath, statsSavePath);
			DebugHelper.Log($"Leaderboard: Saving stats data to file: {path}");
			Dictionary<string, int> stats = new Dictionary<string, int>
			{
				{ "BombsAttempted", BombsAttempted },
				{ "BombsCleared", BombsCleared },
				{ "BombsExploded", BombsExploded }
			};
			string jsonOutput = JsonConvert.SerializeObject(stats, Formatting.Indented, new JsonSerializerSettings()
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			});
			File.WriteAllText(path, jsonOutput);
		}
		catch (FileNotFoundException)
		{
			DebugHelper.LogWarning($"Leaderboard: File {path} was not found.");
		}
		catch (Exception ex)
		{
			DebugHelper.LogException(ex);
		}
	}

	public int Count => _entryList.Count;

	public int SoloCount => _entryListSolo.Count;

	private Dictionary<string, LeaderboardEntry> _entryDictionary = new Dictionary<string, LeaderboardEntry>();
	private List<LeaderboardEntry> _entryList = new List<LeaderboardEntry>();
	private List<LeaderboardEntry> _entryListSolo = new List<LeaderboardEntry>();
	private static Leaderboard _instance;
	private bool _sorted = false;
	public bool Success = false;

	public int BombsAttempted = 0;
	public int BombsCleared = 0;
	public int BombsExploded = 0;
	public int OldBombsAttempted = 0;
	public int OldBombsCleared = 0;
	public int OldBombsExploded = 0;
	public int OldSolves = 0;
	public int OldStrikes = 0;
	public int OldScore = 0;

	public LeaderboardEntry SoloSolver = null;
	public Dictionary<string, int> CurrentSolvers = new Dictionary<string, int>();

	public static int RequiredSoloSolves = 11;
	public static string usersSavePath = "TwitchPlaysUsers.xml";
	public static string statsSavePath = "TwitchPlaysStats.json";

	public static Leaderboard Instance => _instance ?? (_instance = new Leaderboard());
}
