using UnityEngine;

namespace PlasmaCoreSpikestripContent.Core;

public static class PlasmaLog
{
	public static bool debugAll = false;

	private static int sequenceCount = 0;

	private static string[] sequence = new string[30]
	{
		"Memories broken", "The truth goes unspoken", "I've even forgotten my NAAAAAME!", "I don't know the season", "Or what is the reson", "I'm standing here holding my BLAAAAADE!", "A desolate place! (place!)", "Without any trace! (trace!)", "It's only the cold wind I feel", "It's me that I spite",
		"As I stand up and fight", "The only thing I know for real...", "THERE WILL BE", "BLOOD! (BLOOD!)", "SHED! (SHED!)", "THE MAN IN THE MIRROR NODS HIS HEAD!", "THE ONLY ONE!", "LEFT!", "WILL RIDE UPON THE DRAGON'S BACK!", "Because the mountains don't give back what they taaake",
		"OH NO, THERE WILL BE", "BLOOD! (BLOOD!)", "SHED! (SHED!)", "IT'S THE ONLY THING I'VE EVER KNOOOOWN!", "Losing my identityyy", "Wondering, have I gone insane?", "To find the truth in fronta meee", "I must climb this mountain range", "Looking downward from this deadly height", "And never realizing why I fiiiiight!"
	};

	public static void Log(string output, bool allLogOnly = false)
	{
		if (!allLogOnly || debugAll)
		{
			Debug.Log("[SSCPPlugin.PlasmaCore]: " + output);
		}
	}

	public static void LogWarn(string output, bool allLogOnly = false)
	{
		if (!allLogOnly || debugAll)
		{
			Debug.LogWarning("[SSCPPlugin.PlasmaCore]: " + output);
		}
	}

	public static void LogError(string output, bool allLogOnly = false)
	{
		if (!allLogOnly || debugAll)
		{
			Debug.LogError("[SSCPPlugin.PlasmaCore]: " + output);
		}
	}

	public static void LogSequence()
	{
		if (debugAll)
		{
			Debug.LogWarning("[SSCPPlugin.PlasmaCore]: " + sequence[sequenceCount]);
			sequenceCount++;
			if (sequenceCount >= sequence.Length)
			{
				sequenceCount = 0;
			}
		}
	}
}
