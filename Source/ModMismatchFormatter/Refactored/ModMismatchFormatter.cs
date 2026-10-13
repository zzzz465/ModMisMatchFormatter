using Verse;

namespace Madeline.ModMismatchFormatter
{
    public class ModMismatchFormatter : Verse.Mod
    {
        static ModMismatchFormatter instance;
        Settings settings;
        internal static bool useVersionCompare
        {
            get { return instance.settings.useVersionCompare; }
        }
        internal static bool writeMetaToSave
        {
            get { return instance.settings.saveVersionToSaveFile; }
        }
        public ModMismatchFormatter(ModContentPack pack) : base(pack)
        {
            this.settings = GetSettings<Settings>();
            instance = this;
        }

        public override void DoSettingsWindowContents(UnityEngine.Rect inRect)
        {   
			Listing_Standard listingStandard = new Listing_Standard();
			listingStandard.Begin(inRect);

			bool useVersionCompare = this.settings.useVersionCompare;
			string description = "useVersionCompareDescription".Translate(); //"Enable Version checking between saved file and current version. check steam workshop for the detailed description.";
			listingStandard.CheckboxLabeled("EnableVersionChecking".Translate(), ref useVersionCompare, description);

            bool writeMetaToSave = this.settings.saveVersionToSaveFile;
            string writeMetaDescription = "writeMetaDescription".Translate();
            listingStandard.CheckboxLabeled("EnableWritingCustomVersionHeader".Translate(), ref writeMetaToSave, writeMetaDescription);

			this.settings.useVersionCompare = useVersionCompare;
            this.settings.saveVersionToSaveFile = writeMetaToSave;

			listingStandard.End();

			base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "SettingsLabel".Translate();
        }
    }
}