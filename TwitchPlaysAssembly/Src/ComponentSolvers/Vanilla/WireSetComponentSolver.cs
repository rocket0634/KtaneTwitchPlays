﻿using System;
using System.Collections;
using System.Collections.Generic;

public class WireSetComponentSolver : ComponentSolver
{
    public WireSetComponentSolver(BombCommander bombCommander, WireSetComponent bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller) :
        base(bombCommander, bombComponent, ircConnection, canceller)
    {
		_wires = bombComponent.wires;
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {
        if (!inputCommand.StartsWith("cut ", StringComparison.InvariantCultureIgnoreCase))
        {
            yield break;
        }
        inputCommand = inputCommand.Substring(4);
		
		int wireIndex = 0;
        if (!int.TryParse(inputCommand, out wireIndex) || wireIndex < 0 && wireIndex > _wires.Count) yield break;

		yield return null;
		yield return DoInteractionClick(_wires[wireIndex - 1]);
    }

	private List<SnippableWire> _wires;
}
