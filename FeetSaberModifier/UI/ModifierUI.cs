using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Util;

namespace FeetSaberModifier
{
    public class ModifierUI : NotifiableSingleton<ModifierUI>
    {
#pragma warning disable CS0649
        [UIParams]
        BSMLParserParams parserParams;
#pragma warning restore CS0649

        public void updateUI()
        {
            parserParams.EmitEvent("cancel");
        }

        [UIValue("feetSaber")]
        public bool feetSaber
        {
            get => Config.feetSaber;
            set
            {
                Config.feetSaber = value;
                NotifyPropertyChanged(nameof(feetSaber));
                
                if (value && Config.fourSabers)
                {
                    Config.fourSabers = false;
                    NotifyPropertyChanged(nameof(fourSabers));
                }

                Config.topNotesToFeet = value;
                Config.middleNotesToFeet = value;
                Config.bottomNotesToFeet = value;
                NotifyPropertyChanged(nameof(topNotesToFeet));
                NotifyPropertyChanged(nameof(middleNotesToFeet));
                NotifyPropertyChanged(nameof(bottomNotesToFeet));

                Config.Write();
            }
        }

        [UIValue("fourSabers")]
        public bool fourSabers
        {
            get => Config.fourSabers;
            set
            {
                Config.fourSabers = value;
                NotifyPropertyChanged(nameof(fourSabers));

                if (value && Config.feetSaber)
                {
                    Config.feetSaber = false;
                    NotifyPropertyChanged(nameof(feetSaber));
                }

                Config.Write();
            }
        }

        [UIValue("hideSabers")]
        public bool hideSabers
        {
            get => Config.hideSabers;
            set
            {
                Config.hideSabers = value;
                NotifyPropertyChanged(nameof(hideSabers));

                Config.Write();
            }
        }

        [UIValue("onTrackers")]
        public bool onTrackers
        {
            get => Config.onTrackers;
            set
            {
                Config.onTrackers = value;
                NotifyPropertyChanged(nameof(onTrackers));

                Config.Write();
            }
        }

        [UIValue("topNotesToFeet")]
        public bool topNotesToFeet
        {
            get => Config.topNotesToFeet;
            set
            {
                Config.topNotesToFeet = value;
                NotifyPropertyChanged(nameof(topNotesToFeet));

                Config.Write();
            }
        }

        [UIValue("middleNotesToFeet")]
        public bool middleNotesToFeet
        {
            get => Config.middleNotesToFeet;
            set
            {
                Config.middleNotesToFeet = value;
                NotifyPropertyChanged(nameof(middleNotesToFeet));

                Config.Write();
            }
        }

        [UIValue("bottomNotesToFeet")]
        public bool bottomNotesToFeet
        {
            get => Config.bottomNotesToFeet;
            set
            {
                Config.bottomNotesToFeet = value;
                NotifyPropertyChanged(nameof(bottomNotesToFeet));

                Config.Write();
            }
        }

    }
}
