﻿using System;
using System.Collections;

public class SphereShim : ComponentSolverShim
{
	public SphereShim(TwitchModule module)
		: base(module, "sphere")
	{
		ModInfo = ComponentSolverFactory.GetModuleInfo(GetModuleType());
		_component = module.BombComponent.GetComponent(ComponentType);
	}

	protected override IEnumerator RespondToCommandShimmed(string inputCommand)
	{
		yield return RespondToCommandUnshimmed(inputCommand);
	}

	protected override IEnumerator ForcedSolveIEnumeratorShimmed()
	{
		if (Unshimmed.ForcedSolveMethod == null) yield break;
		var coroutine = (IEnumerator) Unshimmed.ForcedSolveMethod.Invoke(Unshimmed.CommandComponent, null);
		while (coroutine.MoveNext())
			yield return coroutine.Current;
		while (_component.GetValue<bool>("checking"))
			yield return true;
	}

	private static readonly Type ComponentType = ReflectionHelper.FindType("theSphereScript", "sphere");

	private readonly object _component;
}