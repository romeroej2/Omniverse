using ff14bot;
using ff14bot.Objects;
using System.ComponentModel;
using System.Configuration;

namespace Boko.Models
{
    public class ChocoboSettingsModel : BaseModel
    {
        private static GameObject Target => Me.CurrentTarget;
        private static LocalPlayer Me => Core.Player;
        private static ChocoboSettingsModel _instance;
        public static ChocoboSettingsModel Instance => _instance ?? (_instance = new ChocoboSettingsModel());

        private ChocoboSettingsModel() : base(@"Settings/" + Me.Name + "/Boko/Chocobo_Settings.json")
        {
        }

        private bool _summonChocobo, _healerThresholdUseDrop, _attackerThresholdUseDrop, _defenderThresholdUseDrop, _freeThresholdUseDrop, _useHealerStance, _useAttackerStance, _useDefenderStance, _useFreeStance, _checkChocoboHealth, _checkPartyHealth, _foodFavored;

        private int _healerThreshold, _attackerThreshold, _defenderThreshold, _freeThreshold;

        [Setting]
        [DefaultValue(false)]
        public bool SummonChocobo
        { get { return _summonChocobo; } set { _summonChocobo = value; OnPropertyChanged(); } }

        [Setting]
        [DefaultValue(false)]
        public bool HealerThresholdUseDrop
        { get { return _healerThresholdUseDrop; } set { _healerThresholdUseDrop = value; OnPropertyChanged(); } }

        [Setting]
        [DefaultValue(false)]
        public bool AttackerThresholdUseDrop
        { get { return _attackerThresholdUseDrop; } set { _attackerThresholdUseDrop = value; OnPropertyChanged(); } }

        [Setting]
        [DefaultValue(false)]
        public bool DefenderThresholdUseDrop
        { get { return _defenderThresholdUseDrop; } set { _defenderThresholdUseDrop = value; OnPropertyChanged(); } }

        [Setting]
        [DefaultValue(false)]
        public bool FreeThresholdUseDrop
        { get { return _freeThresholdUseDrop; } set { _freeThresholdUseDrop = value; OnPropertyChanged(); } }

        [Setting]
        [DefaultValue(0)]
        public int HealerThreshold
        { get { return _healerThreshold; } set { _healerThreshold = value; OnPropertyChanged(); } }

        [Setting]
        [DefaultValue(0)]
        public int AttackerThreshold
        { get { return _attackerThreshold; } set { _attackerThreshold = value; OnPropertyChanged(); } }

        [Setting]
        [DefaultValue(0)]
        public int DefenderThreshold
        { get { return _defenderThreshold; } set { _defenderThreshold = value; OnPropertyChanged(); } }

        [Setting]
        [DefaultValue(0)]
        public int FreeThreshold
        { get { return _freeThreshold; } set { _freeThreshold = value; OnPropertyChanged(); } }

        [Setting]
        [DefaultValue(true)]
        public bool UseHealerStance
        { get { return _useHealerStance; } set { _useHealerStance = value; OnPropertyChanged(); } }

        [Setting]
        [DefaultValue(false)]
        public bool UseAttackerStance
        { get { return _useAttackerStance; } set { _useAttackerStance = value; OnPropertyChanged(); } }

        [Setting]
        [DefaultValue(false)]
        public bool UseDefenderStance
        { get { return _useDefenderStance; } set { _useDefenderStance = value; OnPropertyChanged(); } }

        [Setting]
        [DefaultValue(false)]
        public bool UseFreeStance
        { get { return _useFreeStance; } set { _useFreeStance = value; OnPropertyChanged(); } }

        [Setting]
        [DefaultValue(false)]
        public bool CheckChocoboHealth
        { get { return _checkChocoboHealth; } set { _checkChocoboHealth = value; OnPropertyChanged(); } }

        [Setting]
        [DefaultValue(false)]
        public bool CheckPartyHealth
        { get { return _checkPartyHealth; } set { _checkPartyHealth = value; OnPropertyChanged(); } }

        [Setting]
        [DefaultValue(false)]
        public bool FoodFavored
        { get { return _foodFavored; } set { _foodFavored = value; OnPropertyChanged(); } }

        private volatile FoodMode _foodMode;

        [Setting]
        public FoodMode Food
        { get { return _foodMode; } set { _foodMode = value; OnPropertyChanged(); } }
    }

    public enum FoodMode
    {
        CurielRoot,
        SylkisBud,
        MimettGourd,
        Tantalplant,
        PahsanaFruit,
        None
    }
}