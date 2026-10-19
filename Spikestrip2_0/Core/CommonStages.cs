namespace Spikestrip2_0.Core;

public static class CommonStages
{
	public const string AphelianSanctuary = "ancientloft";

	public const string VoidFields = "arena";

	public const string ArtifactReliquary = "artifactworld";

	public const string DistantRoost = "blackbeach";

	public const string DistantRoostAlt = "blackbeach2";

	public const string AbyssalDepths = "dampcavesimple";

	public const string WetlandAspect = "foggyswamp";

	public const string RallypointDelta = "frozenwall";

	public const string GoldenCoast = "goldshores";

	public const string TitanicPlains = "golemplains";

	public const string TitanicPlainsAlt = "golemplains2";

	public const string AbandonedAqueduct = "goolake";

	public const string Commencement = "moon2";

	public const string SunderedGrove = "rootjungle";

	public const string SirensCall = "shipgraveyard";

	public const string SkyMeadow = "skymeadow";

	public const string SiphonedForest = "snowyforest";

	public const string SulfurPools = "sulfurpools";

	public const string VoidLocus = "voidstage";

	public const string ScorchedAcres = "wispgraveyard";

	public static string Simulacrum(string baseStageName)
	{
		return "it" + baseStageName;
	}
}
