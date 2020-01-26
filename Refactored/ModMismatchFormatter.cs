using Verse;

namespace Madeline.ModMismatchFormatter
{
    public class ModMismatchFormatter : Verse.Mod
    {
        static ModMismatchFormatter modMismatchFormatter;
        Settings settings;
        internal static bool useVersionCompare
        {
            get { return modMismatchFormatter.settings.useVersionCompare; }
        }
        public ModMismatchFormatter(ModContentPack pack) : base(pack)
        {
            this.settings = GetSettings<Settings>();
            modMismatchFormatter = this;
        }

        public override void DoSettingsWindowContents(UnityEngine.Rect inRect)
        {   
			Listing_Standard listingStandard = new Listing_Standard();
			listingStandard.Begin(inRect);
			string description = "Enable Version checking between saved file and current version. check steam workshop for the detailed description.";
			bool result = this.settings.useVersionCompare;
			listingStandard.CheckboxLabeled("EnableVersionChecking".Translate(), ref result, description);
			this.settings.useVersionCompare = result;
			listingStandard.End();
			base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "SettingsLabel".Translate();
        }
    }
}