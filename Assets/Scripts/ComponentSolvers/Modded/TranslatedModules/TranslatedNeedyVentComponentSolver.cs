﻿using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class TranslatedNeedyVentComponentSolver : ComponentSolver
{
    public TranslatedNeedyVentComponentSolver(BombCommander bombCommander, MonoBehaviour bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller) :
        base(bombCommander, bombComponent, ircConnection, canceller)
    {
        _yesButton = (MonoBehaviour)_yesButtonField.GetValue(bombComponent.GetComponent(_needyVentComponentSolverType));
        _noButton = (MonoBehaviour)_noButtonField.GetValue(bombComponent.GetComponent(_needyVentComponentSolverType));
        modInfo = ComponentSolverFactory.GetModuleInfo(GetModuleType());
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {
        if (inputCommand.Equals("y", StringComparison.InvariantCultureIgnoreCase) ||
            inputCommand.Equals("yes", StringComparison.InvariantCultureIgnoreCase) ||
            inputCommand.Equals("press y", StringComparison.InvariantCultureIgnoreCase) ||
            inputCommand.Equals("press yes", StringComparison.InvariantCultureIgnoreCase))
        {
            yield return "yes";
            yield return DoInteractionClick(_yesButton);
        }
        else if (inputCommand.Equals("n", StringComparison.InvariantCultureIgnoreCase) ||
                 inputCommand.Equals("no", StringComparison.InvariantCultureIgnoreCase) ||
                 inputCommand.Equals("press n", StringComparison.InvariantCultureIgnoreCase) ||
                 inputCommand.Equals("press no", StringComparison.InvariantCultureIgnoreCase))
        {
            yield return "no";
            yield return DoInteractionClick(_noButton);
        }
    }

    static TranslatedNeedyVentComponentSolver()
    {
        _needyVentComponentSolverType = ReflectionHelper.FindType("VentGasTranslatedModule");
        _yesButtonField = _needyVentComponentSolverType.GetField("YesButton", BindingFlags.Public | BindingFlags.Instance);
        _noButtonField = _needyVentComponentSolverType.GetField("NoButton", BindingFlags.Public | BindingFlags.Instance);
    }

    private static Type _needyVentComponentSolverType = null;
    private static FieldInfo _yesButtonField = null;
    private static FieldInfo _noButtonField = null;

    private MonoBehaviour _yesButton = null;
    private MonoBehaviour _noButton = null;
}
