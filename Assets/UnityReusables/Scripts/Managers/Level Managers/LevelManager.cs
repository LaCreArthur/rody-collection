using Sirenix.OdinInspector;
using UnityEngine;
using UnityReusables.ScriptableObjects.Events;
using UnityReusables.ScriptableObjects.Variables;
using UnityReusables.Utils.Extensions;

namespace UnityReusables.Managers
{
    /// <summary>
    /// Class that set the IntVariables playerLevelCurrent and setup level prefabs
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] GameObject[] tutoPrefabs = default;
        [SerializeField] GameObject[] levelPrefabs = default;
        [SerializeField] IntVariable playerLevelCurrent = default;
        [SerializeField] StringVariable currentLevelName = default;
        [SerializeField] SimpleEventSO levelInstantiatedEvent;
        [SerializeField] bool levelStartAtOne;
        [SerializeField] bool randomizeLvl;
        [SerializeField] bool forceLevel;
        [SerializeField] GameObject forcedLevel;
        [SerializeField] private bool isLevelsReparented;
        [ShowIf("isLevelsReparented")]
        [SerializeField] Transform levelsParent;
        [SerializeField] int swapPrefabEveryXLevel;
        [SerializeField] BoolVariable tutoCompleted;

        GameObject _lastLvlInstance;
        GameObject _currentLvlInstance;

        GameObject CurrentLvlInstance
        {
            get => _currentLvlInstance;
            set
            {
                _lastLvlInstance = _currentLvlInstance;
                _currentLvlInstance = value;
            }
        }

        void OnEnable()
        {
            playerLevelCurrent.v = EncryptedPlayerPrefs.GetInt(nameof(playerLevelCurrent), 1);
            playerLevelCurrent.AddOnChangeCallback(SetupCurrentLevel);
        }

        void OnDisable() => playerLevelCurrent.RemoveOnChangeCallback(SetupCurrentLevel);

        void Start()
        {
#if !UNITY_EDITOR
            // ensure no forced level are used on build
            forceLevel = false;
#endif
            SetupCurrentLevel();
        }

        public void SetupCurrentLevel()
        {
            EncryptedPlayerPrefs.SetInt(nameof(playerLevelCurrent), playerLevelCurrent.v);
            CurrentLvlInstance = InstantiateLevel();
            if (_lastLvlInstance != null)
            {
                Destroy(_lastLvlInstance);
            }
            levelInstantiatedEvent.Raise();
        }

        GameObject InstantiateLevel()
        {
            var prefab = forceLevel ? forcedLevel : GetSafeLevel();
            if (prefab == null)
            {
                Debug.LogWarning("Level Manager: No Level to instanciate");
                return null;
            }
            var level = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            if (isLevelsReparented) level.transform.parent = levelsParent;
            // track level object names for analytics
            currentLevelName.v = $"{level.name}";
            return level;
        }

        GameObject GetSafeLevel()
        {
            int lvl = playerLevelCurrent.v;
            int realLvl = levelStartAtOne ? lvl - 1 : lvl;
            // check if tuto and level lower than tuto length
            if (tutoPrefabs.Length > 0)
            {
                if (realLvl < tutoPrefabs.Length)
                {
                    if (tutoCompleted.v) tutoCompleted.v = false;
                    return tutoPrefabs[realLvl];
                }
                else
                {
                    if (!tutoCompleted.v) tutoCompleted.v = true;
                    realLvl -= tutoPrefabs.Length;
                }
            }

            if (levelPrefabs.Length <= 0) return null;
            if (randomizeLvl) return levelPrefabs.GetRandom(); 
            if (swapPrefabEveryXLevel > 0)
                realLvl = Mathf.FloorToInt((float) realLvl / swapPrefabEveryXLevel);
            return levelPrefabs[realLvl % levelPrefabs.Length];
        }

        /// <summary>
        /// Should be called after level completed event
        /// </summary>
        public void SetNextLevel()
        {
            playerLevelCurrent.Add(1);
            // if (GetCurrentLevel() >= playerLevelMax.v)
            // {
            //     playerLevelMax.Add(1);
            // }
        }

        #region LevelSelection

        // [SerializeField] IntVariable playerLevelMax = default;


        // public void SetPreviousLevel()
        // {
        //     if (GetCurrentLevel() == 0)
        //     {
        //         Debug.LogWarning("No previous level", this);
        //         return;
        //     }
        //
        //     playerLevelCurrent.Add(-1);
        // }

        // void SetCurrentLevel(int level)
        // {
        //     playerLevelCurrent.v = levelStartAtOne ? level + 1 : level;
        // }

        /// <summary>
        /// Can be used for a level selection UI
        /// </summary>
        /// <param name="level">the new playerCurrentLevel value</param>
        // public void ChangeLevel(int level)
        // {
        //     if (level <= playerLevelMax.v && level > 0)
        //         SetCurrentLevel(level);
        //     else
        //     {
        //         Debug.LogWarning($"Invalid new level {level}", this);
        //     }
        // }
        #endregion
    }
}