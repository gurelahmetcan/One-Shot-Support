using System;
using System.Collections.Generic;
using OneShotSupport.Core;
using UnityEngine;
using OneShotSupport.Data;

namespace OneShotSupport.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject representing a hero in the Guild Merchant system
    /// Heroes are contracted warriors managed by the player, with stats, growth, and contracts
    /// </summary>
    [CreateAssetMenu(fileName = "New Hero", menuName = "One-Shot Support/Hero")]
    public class HeroData : ScriptableObject
    {
        // === METADATA ===
        [Header("Hero Identity")]
        [Tooltip("Display name of the hero")]
        public string heroName;

        [Tooltip("Current age in years")]
        public int currentAge = 20;

        [Tooltip("Life stage based on age")]
        public HeroLifecycleStage lifeStage = HeroLifecycleStage.Rookie;

        // === CORE STATS (5-Stat System) ===
        [Header("Core Stats")]
        [Tooltip("Physical combat, strength, weapon skills")]
        [Range(0, 100)]
        public int might = 10;

        [Tooltip("Persuasion, leadership, negotiation")]
        [Range(0, 100)]
        public int charm = 10;

        [Tooltip("Tactics, problem-solving, investigation")]
        [Range(0, 100)]
        public int wit = 10;

        [Tooltip("Reflexes, stealth, evasion")]
        [Range(0, 100)]
        public int agility = 10;

        [Tooltip("Endurance, resilience, survival")]
        [Range(0, 100)]
        public int fortitude = 50;

        [Tooltip("Current hit points (derived from fortitude)")]
        public int currentHP = 50;

        [Tooltip("Greediness - affects loot demands (reduced by Discipline training)")]
        [Range(0, 100)]
        public int greed = 50;

        // === CONTRACT INFO ===
        [Header("Contract Information")]
        [Tooltip("Total contract length in years")]
        public int contractLengthInYears = 2;

        [Tooltip("Turns remaining in current contract (decreases each turn)")]
        public int turnsRemainingInContract = 8; // 2 years * 4 turns/year

        [Tooltip("Daily salary cost in gold per turn")]
        public int dailySalary = 10;

        [Tooltip("Bond level with the guild (affects negotiations and loyalty)")]
        [Range(0, 10)]
        public int bondLevel = 1;

        // === NEGOTIATION STATE ===
        [Header("Negotiation State")]
        [Tooltip("Whether this hero has walked away from negotiations")]
        public bool hasWalkedAway = false;

        [Tooltip("Turn number when hero walked away (for re-recruitment lockout)")]
        public int walkAwayTurn = -1;

        [Tooltip("Whether hero is locked from recruitment (walked away recently or other reason)")]
        public bool isLockedFromRecruitment = false;

        [Tooltip("Current tension level during negotiation (0-100%)")]
        [Range(0, 100)]
        public int currentTension = 0;

        [Tooltip("Trust level with the guild (0-100%, affects starting tension)")]
        [Range(0, 100)]
        public int trustLevel = 50;

        // === GROWTH DATA ===
        [Header("Growth & Experience")]
        [Tooltip("Current experience points")]
        public int currentXP = 0;

        [Tooltip("Current level")]
        public int level = 1;

        [Tooltip("Aptitudes for stat growth (different per hero)")]
        public HeroAptitudes aptitudes = new HeroAptitudes();

        // === TRAITS ===
        [Header("Traits & Characteristics")]
        [Tooltip("List of traits that modify this hero's stats and behavior")]
        public List<HeroTrait> traits = new List<HeroTrait>();

        // === VISUAL & AUDIO ===
        [Header("Visual & Audio")]
        [Tooltip("Hero portrait for UI display")]
        public Sprite portrait;

        [Tooltip("Hero card sprite for draggable cards in preparation phase")]
        public Sprite cardSprite;

        [Tooltip("Hero voiceline (optional)")]
        public AudioClip heroVoiceline;

        // === EVENTS ===
        public event Action OnHeroDeath;
        public event Action OnContractExpired;
        public event Action<int> OnLevelUp; // passes new level

        // === CALCULATED PROPERTIES ===

        /// <summary>
        /// Check if hero is alive
        /// </summary>
        public bool IsAlive
        {
            get { return currentHP > 0; }
        }

        /// <summary>
        /// Maximum HP is based on fortitude stat
        /// </summary>
        public int MaxHP
        {
            get { return fortitude; }
        }

        /// <summary>
        /// Calculate XP needed for next level (exponential growth)
        /// Formula: 100 * (level ^ 1.5)
        /// </summary>
        public int XPForNextLevel
        {
            get { return Mathf.FloorToInt(100f * Mathf.Pow(level, 1.5f)); }
        }

        /// <summary>
        /// Get total stat modifiers from all traits (5-stat system)
        /// </summary>
        public void GetTotalStatModifiers(out int mightMod, out int charmMod, out int witMod, out int agilityMod, out int fortitudeMod)
        {
            mightMod = 0;
            charmMod = 0;
            witMod = 0;
            agilityMod = 0;
            fortitudeMod = 0;

            foreach (var trait in traits)
            {
                if (trait != null)
                {
                    int m = might, ch = charm, w = wit, a = agility, f = fortitude;
                    trait.ApplyStatModifiers(ref m, ref ch, ref w, ref a, ref f);
                    mightMod += (m - might);
                    charmMod += (ch - charm);
                    witMod += (w - wit);
                    agilityMod += (a - agility);
                    fortitudeMod += (f - fortitude);
                }
            }
        }

        // === CORE METHODS ===

        /// <summary>
        /// Level up the hero with chosen education focus (5-stat system)
        /// Increases stats based on aptitude and focus choice
        /// </summary>
        public void LevelUp(EducationFocus focus)
        {
            level++;

            // Base stat gain per level (modified by aptitude)
            const int baseStatGain = 2;
            const int focusedStatGain = 5;

            // Apply stat gains based on focus and aptitude
            switch (focus)
            {
                case EducationFocus.Might:
                    might += Mathf.FloorToInt(focusedStatGain * aptitudes.mightAptitude);
                    fortitude += Mathf.FloorToInt(baseStatGain * aptitudes.fortitudeAptitude * 0.5f);
                    break;

                case EducationFocus.Charm:
                    charm += Mathf.FloorToInt(focusedStatGain * aptitudes.charmAptitude);
                    wit += Mathf.FloorToInt(baseStatGain * aptitudes.witAptitude * 0.5f);
                    break;

                case EducationFocus.Wit:
                    wit += Mathf.FloorToInt(focusedStatGain * aptitudes.witAptitude);
                    charm += Mathf.FloorToInt(baseStatGain * aptitudes.charmAptitude * 0.5f);
                    break;

                case EducationFocus.Agility:
                    agility += Mathf.FloorToInt(focusedStatGain * aptitudes.agilityAptitude);
                    wit += Mathf.FloorToInt(baseStatGain * aptitudes.witAptitude * 0.5f);
                    break;

                case EducationFocus.Fortitude:
                    int fortitudeGain = Mathf.FloorToInt(focusedStatGain * aptitudes.fortitudeAptitude);
                    fortitude += fortitudeGain;
                    currentHP += fortitudeGain; // Heal on fortitude levelup
                    break;

                case EducationFocus.Discipline:
                    // Discipline training reduces greed
                    int greedReduction = Mathf.FloorToInt(focusedStatGain * aptitudes.disciplineAptitude);
                    greed = Mathf.Max(0, greed - greedReduction);
                    // Discipline training also slightly improves all stats
                    might += 1;
                    charm += 1;
                    Debug.Log($"[HeroData] {heroName} reduced greed by {greedReduction} (now {greed})");
                    break;
            }

            // Clamp all stats to 0-100 range
            might = Mathf.Clamp(might, 0, 100);
            charm = Mathf.Clamp(charm, 0, 100);
            wit = Mathf.Clamp(wit, 0, 100);
            agility = Mathf.Clamp(agility, 0, 100);
            fortitude = Mathf.Clamp(fortitude, 0, 100);
            currentHP = Mathf.Clamp(currentHP, 0, fortitude);

            Debug.Log($"[HeroData] {heroName} leveled up to {level}! Focus: {focus}");
            OnLevelUp?.Invoke(level);
        }

        /// <summary>
        /// Tick the turn - reduce contract time and age the hero
        /// Called once per turn (season)
        /// </summary>
        public void TickTurn()
        {
            // Reduce contract time
            turnsRemainingInContract--;

            // Age hero every 4 turns (1 year)
            // Note: If using HeroLifecycleManager, this might be handled externally
            // For now, we track internally as well
            if (turnsRemainingInContract % 4 == 0)
            {
                currentAge++;
                UpdateLifeStage();
            }

            // Check if contract expired
            if (turnsRemainingInContract <= 0)
            {
                OnContractExpired?.Invoke();
            }
        }

        /// <summary>
        /// Check if contract has expired
        /// </summary>
        public bool CheckContractStatus()
        {
            return turnsRemainingInContract <= 0;
        }

        /// <summary>
        /// Apply damage to the hero
        /// Returns true if hero died
        /// </summary>
        public bool TakeDamage(int amount)
        {
            currentHP -= amount;
            currentHP = Mathf.Max(0, currentHP);

            Debug.Log($"[HeroData] {heroName} took {amount} damage. HP: {currentHP}/{MaxHP}");

            // Check for death
            if (currentHP <= 0)
            {
                Debug.LogWarning($"[HeroData] {heroName} has died!");
                OnHeroDeath?.Invoke();
                return true;
            }

            // Check for forced retirement (low HP + old age)
            if (currentHP < MaxHP * 0.2f && lifeStage == HeroLifecycleStage.Veteran)
            {
                Debug.LogWarning($"[HeroData] {heroName} is considering retirement due to injury and age...");
                // Could trigger retirement event here
            }

            return false;
        }

        /// <summary>
        /// Heal the hero
        /// </summary>
        public void Heal(int amount)
        {
            currentHP += amount;
            currentHP = Mathf.Min(currentHP, MaxHP);
        }

        /// <summary>
        /// Fully heal the hero to max HP
        /// </summary>
        public void FullHeal()
        {
            currentHP = MaxHP;
        }

        /// <summary>
        /// Add experience points and check for level up
        /// Returns true if leveled up
        /// </summary>
        public bool AddXP(int xpAmount)
        {
            currentXP += xpAmount;

            // Check if enough XP for level up
            if (currentXP >= XPForNextLevel)
            {
                currentXP -= XPForNextLevel;
                return true; // Ready to level up
            }

            return false;
        }

        /// <summary>
        /// Update life stage based on current age
        /// Uses default age ranges if HeroLifecycleManager not available
        /// </summary>
        public void UpdateLifeStage()
        {
            if (currentAge <= 25)
                lifeStage = HeroLifecycleStage.Rookie;
            else if (currentAge <= 35)
                lifeStage = HeroLifecycleStage.Prime;
            else if (currentAge <= 45)
                lifeStage = HeroLifecycleStage.Veteran;
            else
                lifeStage = HeroLifecycleStage.Retired;
        }

        /// <summary>
        /// Add a trait to this hero
        /// </summary>
        public void AddTrait(HeroTrait trait)
        {
            if (trait != null && !traits.Contains(trait))
            {
                traits.Add(trait);
                Debug.Log($"[HeroData] {heroName} gained trait: {trait.traitName}");
            }
        }

        /// <summary>
        /// Remove a trait from this hero
        /// </summary>
        public void RemoveTrait(HeroTrait trait)
        {
            if (traits.Contains(trait))
            {
                traits.Remove(trait);
                Debug.Log($"[HeroData] {heroName} lost trait: {trait.traitName}");
            }
        }

        /// <summary>
        /// Get modified daily salary after trait modifiers
        /// </summary>
        public float GetEffectiveSalary()
        {
            float salary = dailySalary;
            foreach (var trait in traits)
            {
                if (trait != null)
                {
                    salary = trait.ApplySalaryModifier(salary);
                }
            }
            return salary;
        }

        /// <summary>
        /// Initialize a new hero with random stats (5-stat system)
        /// </summary>
        public void InitializeRandom(string name, int age, HeroAptitudes randomAptitudes, int contractYears = 2)
        {
            heroName = name;
            currentAge = age;
            UpdateLifeStage();

            // Random base stats (5-stat system, scaled to 0-100)
            might = UnityEngine.Random.Range(10, 30);
            charm = UnityEngine.Random.Range(10, 30);
            wit = UnityEngine.Random.Range(10, 30);
            agility = UnityEngine.Random.Range(10, 30);
            fortitude = UnityEngine.Random.Range(30, 60);
            currentHP = fortitude;
            greed = UnityEngine.Random.Range(30, 70);

            // Set aptitudes
            aptitudes = randomAptitudes;

            // Set contract
            contractLengthInYears = contractYears;
            turnsRemainingInContract = contractYears * 4; // 4 turns per year

            // Random salary based on average stats
            int avgStats = (might + charm + wit + agility + fortitude) / 5;
            dailySalary = Mathf.RoundToInt(avgStats * 0.5f); // Salary scales with stat average

            // Start at level 1
            level = 1;
            currentXP = 0;
            bondLevel = 1;

            // Initialize negotiation state
            hasWalkedAway = false;
            walkAwayTurn = -1;
            isLockedFromRecruitment = false;
            currentTension = 0;
            trustLevel = UnityEngine.Random.Range(40, 61); // Random initial trust 40-60

            Debug.Log($"[HeroData] Initialized random hero: {heroName}, Age: {currentAge}, Stage: {lifeStage}, Stats: M{might}/C{charm}/W{wit}/A{agility}/F{fortitude}");
        }

        // === NEGOTIATION METHODS ===

        /// <summary>
        /// Mark hero as walked away from negotiations
        /// </summary>
        public void MarkAsWalkedAway(int currentTurn)
        {
            hasWalkedAway = true;
            walkAwayTurn = currentTurn;
            isLockedFromRecruitment = true;
            Debug.LogWarning($"[HeroData] {heroName} walked away on turn {currentTurn}");
        }

        /// <summary>
        /// Check if enough turns have passed to allow re-recruitment
        /// Heroes disappear during Annual Refresh (every 4 turns)
        /// </summary>
        public bool CanBeReRecruited(int currentTurn, int lockoutTurns = 4)
        {
            if (!hasWalkedAway)
                return true;

            int turnsSinceWalkAway = currentTurn - walkAwayTurn;
            return turnsSinceWalkAway >= lockoutTurns;
        }

        /// <summary>
        /// Reset walk-away status (called during annual refresh)
        /// </summary>
        public void ResetWalkAwayStatus()
        {
            hasWalkedAway = false;
            walkAwayTurn = -1;
            isLockedFromRecruitment = false;
            currentTension = 0;
            Debug.Log($"[HeroData] {heroName} walk-away status reset");
        }

        /// <summary>
        /// Initialize negotiation state based on trust level
        /// </summary>
        public void InitializeNegotiation()
        {
            // Calculate starting tension based on trust
            if (ContractNegotiationManager.Instance != null)
            {
                currentTension = ContractNegotiationManager.Instance.CalculateStartingTension(trustLevel);
                Debug.Log($"[HeroData] {heroName} negotiation started with {currentTension}% tension (Trust: {trustLevel}%)");
            }
            else
            {
                Debug.LogWarning("[HeroData] ContractNegotiationManager not found, using default tension");
                currentTension = 0;
            }
        }
    }
}
