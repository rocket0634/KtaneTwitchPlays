﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class BombCommands
{
	#region Commands
	[Command("help")]
	public static void Help(string user, bool isWhisper) => IRCConnection.SendMessage(TwitchPlaySettings.data.BombHelp, user, !isWhisper);

	[Command(@"(turn|turn round|turn around|rotate|flip|spin)")]
	public static IEnumerator TurnBomb(TwitchBomb bomb) => bomb.TurnBomb();

	[Command(@"(hold|pick up)")]
	public static IEnumerator Hold(TwitchBomb bomb) => bomb.HoldBomb();
	[Command(@"(drop|let go|put down)")]
	public static IEnumerator Drop(TwitchBomb bomb) => bomb.LetGoBomb();

	[Command(@"edgework((?: right| left| back| r| l| b)?)"), ElevatorOnly]
	public static IEnumerator EdgeworkElevator(TwitchBomb bomb, [Group(1)] string edge, string user, bool isWhisper) => Edgework(bomb, edge, user, isWhisper);
	[Command(@"edgework((?: 45|-45)|(?: top right| right top| right bottom| bottom right| bottom left| left bottom| left top| top left| left| top| right| bottom| tr| rt| tl| lt| br| rb| bl| lb| t| r| b| l))?"), ElevatorDisallowed]
	public static IEnumerator Edgework(TwitchBomb bomb, [Group(1)] string edge, string user, bool isWhisper)
	{
		if (TwitchPlaySettings.data.EnableEdgeworkCommand || TwitchPlaySettings.data.AnarchyMode)
			return bomb.ShowEdgework(edge);
		else
		{
			IRCConnection.SendMessage(string.Format(TwitchPlaySettings.data.BombEdgework, bomb.EdgeworkText.text), user, !isWhisper);
			return null;
		}
	}

	[Command(@"(timer?|clock)")]
	public static void Time(TwitchBomb bomb, string user, bool isWhisper) => IRCConnection.SendMessage(string.Format(TwitchPlaySettings.data.BombTimeRemaining, bomb.GetFullFormattedTime, bomb.GetFullStartingTime), user, !isWhisper);
	[Command(@"(timestamp|date)")]
	public static void Timestamp(TwitchBomb bomb, string user, bool isWhisper) => IRCConnection.SendMessage(string.Format(TwitchPlaySettings.data.BombTimeStamp, bomb.BombTimeStamp), user, !isWhisper);

	[Command(@"endzenmode")]
	public static IEnumerator Explode(TwitchBomb bomb, string user, bool isWhisper)
	{
		if (!OtherModes.ZenModeOn)
		{
			IRCConnection.SendMessage("Zen mode is not on.", user, false, user);
			return null;
		}

		if (isWhisper)
		{
			IRCConnection.SendMessage("Sorry {0}, you can't end Zen mode in a whisper.", user, false, user);
			return null;
		}

		Leaderboard.Instance.GetRank(user, out var entry);
		if (entry.SolveScore < TwitchPlaySettings.data.MinScoreForNewbomb && !UserAccess.HasAccess(user, AccessLevel.Defuser, true))
		{
			IRCConnection.SendMessage("Sorry, you don't have enough points to end Zen mode.");
			return null;
		}

		return bomb.DelayBombExplosionCoroutine();
	}

	[Command(@"(explode|detonate)", AccessLevel.Mod, AccessLevel.Mod)]
	public static IEnumerator Explode(TwitchBomb bomb, [Group(1)] string cmd, string user, bool isWhisper) => bomb.DelayBombExplosionCoroutine();

	[Command(@"(status|info)")]
	public static void Status(TwitchBomb bomb, string user, bool isWhisper)
	{
		int currentReward = TwitchPlaySettings.GetRewardBonus();
		if (OtherModes.TimeModeOn)
		{
			IRCConnection.SendMessage(string.Format(TwitchPlaySettings.data.BombStatusTimeMode, bomb.GetFullFormattedTime, bomb.GetFullStartingTime,
				OtherModes.GetAdjustedMultiplier(), bomb.bombSolvedModules, bomb.bombSolvableModules, currentReward), user, !isWhisper);
		}
		else if (OtherModes.VSModeOn)
		{
			IRCConnection.SendMessage(string.Format(TwitchPlaySettings.data.BombStatusVsMode, bomb.GetFullFormattedTime,
				bomb.GetFullStartingTime, OtherModes.teamHealth, OtherModes.bossHealth, currentReward), user, !isWhisper);
		}
		else
		{
			IRCConnection.SendMessage(string.Format(TwitchPlaySettings.data.BombStatus, bomb.GetFullFormattedTime, bomb.GetFullStartingTime,
				bomb.StrikeCount, bomb.StrikeLimit, bomb.bombSolvedModules, bomb.bombSolvableModules, currentReward), user, !isWhisper);
		}
	}

	[Command(@"pause", AccessLevel.Admin)]
	public static void Pause(TwitchBomb bomb)
	{
		if (bomb.Bomb.GetTimer().IsUpdating)
		{
			OtherModes.DisableLeaderboard();
			bomb.Bomb.GetTimer().StopTimer();
		}
	}
	[Command(@"(unpause|resume)", AccessLevel.Admin)]
	public static void Unpause(TwitchBomb bomb)
	{
		if (!bomb.Bomb.GetTimer().IsUpdating)
			bomb.Bomb.GetTimer().StartTimer();
	}

	[Command(@"(?:add|increase|(subtract|decrease|remove)|(change|set)) +(?:time|t) +(.+)", AccessLevel.Admin, AccessLevel.Admin)]
	public static void ChangeTimer(TwitchBomb bomb, string user, bool isWhisper, [Group(1)] bool negative, [Group(2)] bool direct, [Group(3)] bool isTime, [Group(4)] bool isStrikes, [Group(5)] string amount)
	{
		float time = 0;
		float originalTime = bomb.Bomb.GetTimer().TimeRemaining;
		var timeLengths = new Dictionary<string, float>()
			{
				{ "ms", 0.001f },
				{ "s", 1 },
				{ "m", 60 },
				{ "h", 3600 },
				{ "d", 86400 },
				{ "w", 604800 },
				{ "y", 31536000 },
			};

		foreach (string part in amount.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
		{
			bool valid = false;
			foreach (string unit in timeLengths.Keys)
			{
				if (!part.EndsWith(unit, StringComparison.InvariantCultureIgnoreCase) || !float.TryParse(part.Substring(0, part.Length - unit.Length), out float length))
					continue;
				time += length * timeLengths[unit];
				valid = true;
				break;
			}

			if (!valid)
			{
				IRCConnection.SendMessage(@"I don’t understand “{0}”.", user, !isWhisper, part);
				return;
			}
		}

		time = (float) Math.Round((decimal) time, 2, MidpointRounding.AwayFromZero);
		if (!direct && Math.Abs(time) < 0.01f)
			return;

		bomb.CurrentTimer = direct ? time : negative ? bomb.CurrentTimer - time : bomb.CurrentTimer + time;

		if (originalTime < bomb.CurrentTimer)
			OtherModes.DisableLeaderboard(true);

		IRCConnection.SendMessage(direct
			? $"Set the bomb's timer to {Math.Abs(time < 0 ? 0 : time).FormatTime()}."
			: $"{(time > 0 ? "Added" : "Subtracted")} {Math.Abs(time).FormatTime()} {(time > 0 ? "to" : "from")} the timer.", user, !isWhisper);
	}

	[Command(@"(?:add|increase|(subtract|decrease|remove)|(change|set)) +(?:(strikes?|s)|strikelimit|sl|maxstrikes?|ms) +(-?\d+)", AccessLevel.Admin, AccessLevel.Admin)]
	public static void ChangeStrikeParameter(TwitchBomb bomb, string user, bool isWhisper, [Group(1)] bool negative, [Group(2)] bool direct, [Group(3)] bool isStrikes, [Group(4)] int amount)
	{
		void setParameter(string thing1, string thing2, int originalAmount, Func<int, int> set)
		{
			// Don’t go below 0 strikes because Simon Says is unsolvable then.
			var newAmount = set(Math.Max(0, direct ? amount : negative ? originalAmount - amount : originalAmount + amount));

			if (newAmount < originalAmount)
				OtherModes.DisableLeaderboard(true);

			if (direct)
				IRCConnection.SendMessage(string.Format("{2} set to {0} {1}.", newAmount, newAmount != 1 ? "strikes" : "strike", thing1), user, !isWhisper);
			else
			{
				var difference = Math.Abs(newAmount - originalAmount);
				IRCConnection.SendMessage(string.Format(newAmount >= originalAmount ? "Added {0} {1} to the {2}." : "Subtracted {0} {1} from the {2}.", difference, difference != 1 ? "strikes" : "strike", thing2), user, !isWhisper);
			}
		}

		if (isStrikes)
			setParameter("Strike count", "bomb", bomb.StrikeCount, am => bomb.StrikeCount = am);
		else    // strike limit
			setParameter("Strike limit", "strike limit", bomb.StrikeLimit, am => bomb.StrikeLimit = am);

		TwitchGame.ModuleCameras.UpdateStrikes();
	}
	#endregion
}
