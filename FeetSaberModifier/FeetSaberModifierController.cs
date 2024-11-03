//for my mod video
//change ninjasaber mode to feetsaber 2-player. no meaning without 2-player mod

using BS_Utils.Gameplay;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

namespace FeetSaberModifier
{
    public class FeetSaberModifierController : MonoBehaviour
    {
        public static FeetSaberModifierController instance { get; private set; }

        public bool inGame = false;

        private AudioTimeSyncController _audioTimeSyncController;
        private AudioSource _audioSource;
        private PauseController _pauseController;

        private SaberManager _saberManager;
        private NoteCutter _noteCutter;
        private PlayerTransforms _playerTransform;
        private Transform _rightSaberTransform;
        private Transform _leftSaberTransform;

        private SaberClashChecker _saberClashChecker;
        private SaberBurnMarkArea _saberBurnMarkArea;
        private SaberBurnMarkSparkles _saberBurnMarkSparkles;

        private Transform _avatarFootL;
        private Transform _avatarFootR;
        private GameObject _feetSaber;

        private Saber _saberFootL;
        private Saber _saberFootR;

        private InputDevice leftFootTracker = new InputDevice();
        private InputDevice rightFootTracker = new InputDevice();

        private bool _init;

        internal static readonly string saberFootLName = "saberFootL";
        internal static readonly string saberFootRName = "saberFootR";

        private bool FindTrackers()
        {
            Logger.log.Debug("Finding trackers");
            try
            {
                var trackers = new List<(InputDevice device, float x, float y)>();

                List<InputDevice> allDevices = new List<InputDevice>();
                InputDevices.GetDevices(allDevices);
                foreach (InputDevice device in allDevices)
                {
                    Logger.log.Debug($"name={device.name}, manufacturer={device.manufacturer}, serial={device.serialNumber}, isValid={device.isValid}, characteristics={device.characteristics}");

                    if (device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position) &&
                        device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
                    {
                        Logger.log.Debug($"  position=({position.x}, {position.y}, {position.z})");
                        Logger.log.Debug($"  rotation=({rotation.x}, {rotation.y}, {rotation.z})");

                        if (((device.characteristics & InputDeviceCharacteristics.TrackedDevice) == InputDeviceCharacteristics.TrackedDevice) &&
                            device.isValid &&
                            (!(position.x == 0 && position.y == 0 && position.z == 0)))
                        {
                            trackers.Add((device, position.x, position.y));
                        }
                    }
                }

                if (trackers.Count < 2)
                {
                    Logger.log.Debug($"Trackers not found");
                    return false;
                }

                var ordered = trackers.OrderBy(tracker => tracker.y).ToArray();
                if (ordered[0].x < ordered[1].x)
                {
                    leftFootTracker = ordered[0].device;
                    rightFootTracker = ordered[1].device;
                }
                else
                {
                    leftFootTracker = ordered[1].device;
                    rightFootTracker = ordered[0].device;
                }

                Logger.log.Debug($"leftFootTracker: " + $"name={leftFootTracker.name}, serial={leftFootTracker.serialNumber}");
                Logger.log.Debug($"rightFootTracker: " + $"name={rightFootTracker.name}, serial={rightFootTracker.serialNumber}");

                return true;
            }
            catch
            {
                Logger.log.Debug($"Exception on FindTrackers");
                return false;
            }
        }

        private bool FindAvatar()
        {
            Animator animator = FindObjectsOfType<Animator>().FirstOrDefault(a => a.avatar != null && a.avatar.isHuman);

            if (animator == null)
            {
                Logger.log.Debug("Avatar NotFound");
                return false;
            }

            Logger.log.Debug("Avatar Found");

            _avatarFootL = animator.GetBoneTransform(HumanBodyBones.LeftFoot)?.transform;
            _avatarFootR = animator.GetBoneTransform(HumanBodyBones.RightFoot)?.transform;

            if (_avatarFootL == null || _avatarFootR == null)
            {
                Logger.log.Debug("Foot not found");
                return false;
            }

            Logger.log.Debug($"FootL: {_avatarFootL.position.x}, {_avatarFootL.position.y}, {_avatarFootL.position.z}");
            Logger.log.Debug($"FootR: {_avatarFootR.position.x}, {_avatarFootR.position.y}, {_avatarFootR.position.z}");

            return true;
        }

        private void Awake()
        {
            if (instance != null)
            {
                Logger.log?.Warn($"Instance of {this.GetType().Name} already exists, destroying.");
                GameObject.DestroyImmediate(this);
                return;
            }
            GameObject.DontDestroyOnLoad(this); // Don't destroy this object on scene changes
            instance = this;
            Logger.log?.Debug($"{name}: Awake()");

            FindTrackers();
        }

        private void OnEnable()
        {
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        public void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            Logger.log.Debug($"OnActiveSceneChanged: {newScene.name}");

            _init = false;
            inGame = false;

            if (newScene.name == "GameCore")
            {
                inGame = true;

                Logger.log.Debug($"Mode: {BS_Utils.Plugin.LevelData.Mode}");
                if (BS_Utils.Plugin.LevelData.Mode != BS_Utils.Gameplay.Mode.Standard)
                {
                    Config.feetSaber = false;
                    Config.onTrackers = false;

                    Config.fourSabers = false;
                    Config.topNotesToFeet = false;
                    Config.middleNotesToFeet = false;
                    Config.bottomNotesToFeet = false;

                    Config.hideSabers = false;

                    ModifierUI.instance.updateUI();

                    return;
                }

                Config.Read();

                if (Config.feetSaber || Config.fourSabers || Config.topNotesToFeet || Config.middleNotesToFeet || Config.bottomNotesToFeet)
                {
                    ScoreSubmission.DisableSubmission(Plugin.Name);
                }

                if (Config.feetSaber || Config.fourSabers)
                {
                    if (_feetSaber == null)
                    {
                        _feetSaber = new GameObject("FeetSaber");
                    }
                }

                StartCoroutine(OnGameCoreCoroutine());
            }
            else if (newScene.name == "MainMenu")
            {
                OnMainMenu();
            }
        }

        public void OnPause()
        {
            if (_saberFootL != null) _saberFootL.gameObject.SetActive(false);
            if (_saberFootR != null) _saberFootR.gameObject.SetActive(false);
        }

        public void OnPauseResume()
        {
            if (_saberFootL != null) _saberFootL.gameObject.SetActive(true);
            if (_saberFootR != null) _saberFootR.gameObject.SetActive(true);
        }

        private void EnumChildren(GameObject parent, string indent)
        {
            Logger.log.Debug(indent + parent.name);
            foreach (Transform transform in parent.GetComponentInChildren<Transform>())
            {
                EnumChildren(transform.gameObject, indent + "  ");
            }
        }

        private IEnumerator OnGameCoreCoroutine()
        {
            yield return null;

            if (_audioTimeSyncController == null)
                _audioTimeSyncController = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().FirstOrDefault();
            _audioSource = _audioTimeSyncController.GetPrivateField<AudioSource>("_audioSource");

            if (_pauseController == null)
            {
                _pauseController = Resources.FindObjectsOfTypeAll<PauseController>().FirstOrDefault();
                _pauseController.didPauseEvent += OnPause;
                _pauseController.didResumeEvent += OnPauseResume;
            }

            // wait for CustomSaber mod
            yield return new WaitUntil(() => FindObjectsOfType<Saber>().Any());
            yield return new WaitForSecondsRealtime(0.1f);

            if (_saberManager == null)
                _saberManager = FindObjectsOfType<SaberManager>().FirstOrDefault();
            if (_noteCutter == null)
            {
                CuttingManager cuttingManager = FindObjectsOfType<CuttingManager>().FirstOrDefault();
                _noteCutter = cuttingManager.GetPrivateField<NoteCutter>("_noteCutter");
            }
            if (_playerTransform == null)
                _playerTransform = FindObjectsOfType<PlayerTransforms>().FirstOrDefault();
            if (_rightSaberTransform == null)
                _rightSaberTransform = _saberManager.rightSaber.transform;
            if (_leftSaberTransform == null)
                _leftSaberTransform = _saberManager.leftSaber.transform;
            if (_saberClashChecker == null)
            {
                SaberClashEffect saberClashEffect = FindObjectsOfType<SaberClashEffect>().FirstOrDefault();
                if (saberClashEffect != null)
                {
                    _saberClashChecker = saberClashEffect.GetPrivateField<SaberClashChecker>("_saberClashChecker");
                }
            }
            if (_saberBurnMarkArea == null)
                _saberBurnMarkArea = Resources.FindObjectsOfTypeAll<SaberBurnMarkArea>().FirstOrDefault();
            if (_saberBurnMarkSparkles == null)
                _saberBurnMarkSparkles = Resources.FindObjectsOfTypeAll<SaberBurnMarkSparkles>().FirstOrDefault();

            if (Config.hideSabers)
            {
                SetSaberVisible(_saberManager.rightSaber, false);
                SetSaberVisible(_saberManager.leftSaber, false);
                SetTrailWidth(0f);
                HarmonyPatches.SaberClashCheckerAreSabersClashing.disabled = true;
                _saberBurnMarkArea.enabled = false;
                _saberBurnMarkSparkles.enabled = false;
            }

            if (Config.feetSaber || Config.fourSabers)
            {
                StartCoroutine(SetupSabersCoroutine());
            }

            if (_audioTimeSyncController == null ||
                _audioSource == null ||
                _pauseController == null ||
                _saberManager == null ||
                _noteCutter == null ||
                _playerTransform == null ||
                _rightSaberTransform == null ||
                _leftSaberTransform == null ||
                _saberClashChecker == null ||
                _saberBurnMarkArea == null ||
                _saberBurnMarkSparkles == null)
            {
                Logger.log.Debug("GameCore Init Fail");
                Logger.log.Debug($"{_audioTimeSyncController}, {_audioSource}, {_pauseController}, {_noteCutter}, {_playerTransform}, {_rightSaberTransform}, {_leftSaberTransform}, {_saberClashChecker}, {_saberBurnMarkArea}, {_saberBurnMarkSparkles}");
            }
            else
            {
                Logger.log.Debug("GameCore Init Success");
            }

            _init = true;
        }

        private IEnumerator SetupSabersCoroutine()
        {
            bool trackerFound = FindTrackers();

            // find avatar if nessesary
            if (!(trackerFound && Config.onTrackers))
            {
                // need some wait
                for (int i = 0; i < 3; i++)
                {
                    bool avatarFound = FindAvatar();

                    if (avatarFound)
                        break;

                    yield return new WaitForSecondsRealtime(0.1f);
                }
            }

            // setup sabers if tracker/foot found
            if ((_avatarFootL != null && _avatarFootR != null) || (leftFootTracker != null && rightFootTracker != null))
            {
                if (Config.fourSabers)
                {
                    _saberFootL = CopySaber(_saberManager.leftSaber);
                    _saberFootR = CopySaber(_saberManager.rightSaber);
                    _saberFootL.transform.localScale = new Vector3(2, 2, 0.25f);
                    _saberFootR.transform.localScale = new Vector3(2, 2, 0.25f);

                    _saberFootL.name = saberFootLName;
                    _saberFootR.name = saberFootRName;
                }
                else
                {
                    SetTrailWidth(0f);
                    _saberManager.rightSaber.transform.localScale = new Vector3(2, 2, 0.25f);
                    _saberManager.leftSaber.transform.localScale = new Vector3(2, 2, 0.25f);
                }
            }
        }

        private Saber CopySaber(Saber saber)
        {
            Saber result = Instantiate(saber);
            result.transform.SetParent(saber.transform.parent, false);
            result.transform.localPosition = Vector3.zero;
            result.transform.localRotation = Quaternion.identity;

            SaberModelContainer saberModelContainer = result.GetComponent<SaberModelContainer>();
            DestroyImmediate(saberModelContainer);

            VRController vrController = result.GetComponent<VRController>();
            DestroyImmediate(vrController);

            // TODO: SaberTrail wont function properly
            //SaberTrail[] originalTrails = saber.GetComponentsInChildren<SaberTrail>();
            //SaberTrail[] newSaberTrails = result.GetComponentsInChildren<SaberTrail>();

            //for (int i = 0; i < originalTrails.Length; i++)
            //{
            //    Color color = originalTrails[i].GetPrivateField<Color>("_color");
            //    IBladeMovementData bladeMovementData = originalTrails[i].GetPrivateField<IBladeMovementData>("_movementData");
            //    newSaberTrails[i].Setup(color, bladeMovementData);
            //}

            return result;
        }

        private void OnMainMenu()
        {
            // cleanup
            if (_feetSaber != null) Destroy(_feetSaber);
            if (_saberFootL != null) Destroy(_saberFootL);
            if (_saberFootR != null) Destroy(_saberFootR);
        }

        private void Start()
        {
            Logger.log?.Debug($"{name}: Start()");
        }

        private void AdjustPosAndRot(Transform transform, float posX, float posY, float posZ, float rotX, float rotY, float rotZ)
        {
            transform.Translate(posX * 0.01f, posY * 0.01f, posZ * 0.01f, Space.Self);
            transform.Rotate(rotX, 0, 0, Space.Self);
            transform.Rotate(0, rotY, 0, Space.Self);
            transform.Rotate(0, 0, rotZ, Space.Self);
        }

        private void UpdateAdditionalSaber(Saber saber)
        {
            saber.ManualUpdate();
            if (_noteCutter != null)
            {
                _noteCutter.Cut(saber);
            }
        }

        private void Update()
        {
            if (!_init)
            {
                return;
            }

            if (!(Config.feetSaber || Config.fourSabers))
                return;

            if ((_avatarFootL == null || _avatarFootR == null) && (leftFootTracker == null || rightFootTracker == null))
                return;

            // get foot saber transforms

            Transform footSaberL = null;
            Transform footSaberR = null;

            if (Config.fourSabers)
            {
                if (_saberFootL == null || _saberFootR == null)
                {
                    Logger.log.Debug("foot saber not found");
                    return;
                }

                footSaberL = _saberFootL.transform;
                footSaberR = _saberFootR.transform;
            }
            else
            {
                footSaberL = _leftSaberTransform;
                footSaberR = _rightSaberTransform;
            }

            // get foot transforms

            Quaternion footRotationL;
            Quaternion footRotationR;
            Vector3 footPositionL;
            Vector3 footPositionR;

            bool useTrackers = (Config.onTrackers && !(leftFootTracker == null || rightFootTracker == null)) || (_avatarFootL == null || _avatarFootR == null);

            if (useTrackers)
            {
                leftFootTracker.TryGetFeatureValue(CommonUsages.devicePosition, out footPositionL);
                rightFootTracker.TryGetFeatureValue(CommonUsages.devicePosition, out footPositionR);
                leftFootTracker.TryGetFeatureValue(CommonUsages.deviceRotation, out footRotationL);
                rightFootTracker.TryGetFeatureValue(CommonUsages.deviceRotation, out footRotationR);
            }
            else
            {
                footPositionL = _avatarFootL.position;
                footPositionR = _avatarFootR.position;
                footRotationL = _avatarFootL.rotation;
                footRotationR = _avatarFootR.rotation;
            }

            // update foot saber transforms

            footSaberL.position = footPositionL;
            footSaberR.position = footPositionR;
            footSaberL.rotation = footRotationL;
            footSaberR.rotation = footRotationR;

            // adjust saber position and rotation

            if (useTrackers)
            {
                AdjustPosAndRot(footSaberL,
                    -1 * Config.trackerFootPosX, Config.trackerFootPosY, Config.trackerFootPosZ,
                    Config.trackerFootRotX, -1 * Config.trackerFootRotY, -1 * Config.trackerFootRotZ);
                AdjustPosAndRot(footSaberR,
                    Config.trackerFootPosX, Config.trackerFootPosY, Config.trackerFootPosZ,
                    Config.trackerFootRotX, Config.trackerFootRotY, Config.trackerFootRotZ);
            }
            else
            {
                AdjustPosAndRot(footSaberL,
                    -1 * Config.avatarFootPosX, Config.avatarFootPosY, Config.avatarFootPosZ,
                    Config.avatarFootRotX, -1 * Config.avatarFootRotY, -1 * Config.avatarFootRotZ);
                AdjustPosAndRot(footSaberR,
                    Config.avatarFootPosX, Config.avatarFootPosY, Config.avatarFootPosZ,
                    Config.avatarFootRotX, Config.avatarFootRotY, Config.avatarFootRotZ);
            }
            
            // update saber events

            if (Config.fourSabers)
            {
                UpdateAdditionalSaber(_saberFootL);
                UpdateAdditionalSaber(_saberFootR);
            }
        }

        private void OnDestroy()
        {
            Logger.log?.Debug($"{name}: OnDestroy()");
            instance = null;
        }

        private void SetSaberVisible(Saber saber, bool active)
        {
            IEnumerable<MeshFilter> meshFilters = saber.transform.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter meshFilter in meshFilters)
            {
                meshFilter.gameObject.SetActive(active);

                MeshFilter filter = meshFilter.GetComponentInChildren<MeshFilter>();
                if (filter != null)
                {
                    filter.gameObject.SetActive(active);
                }
            }

            IEnumerable<Renderer> renders = saber.transform.GetComponentsInChildren<Renderer>();
            foreach (Renderer render in renders)
            {
                render.gameObject.SetActive(active);

                Renderer r = render.GetComponentInChildren<Renderer>();
                if (r != null)
                {
                    r.gameObject.SetActive(active);
                }
            }
        }

        private Coroutine _setTrailWidthCoroutine;

        private IEnumerator SetTrailWidthCoroutine(float trailWidth)
        {
            yield return new WaitUntil(() => Resources.FindObjectsOfTypeAll<SaberTrailRenderer>().Any());

            // need some wait. 0.02 is not enough for my environment.
            yield return new WaitForSecondsRealtime(0.03f);
            foreach (SaberTrailRenderer trail in Resources.FindObjectsOfTypeAll<SaberTrailRenderer>())
            {
                trail.SetPrivateField("_trailWidth", trailWidth);
            }

            // just in case
            yield return new WaitForSecondsRealtime(0.1f);
            foreach (SaberTrailRenderer trail in Resources.FindObjectsOfTypeAll<SaberTrailRenderer>())
            {
                trail.SetPrivateField("_trailWidth", trailWidth);
            }
        }

        private void SetTrailWidth(float trailWidth)
        {
            if (_setTrailWidthCoroutine != null)
            {
                StopCoroutine(_setTrailWidthCoroutine);
            }
            _setTrailWidthCoroutine = StartCoroutine(SetTrailWidthCoroutine(trailWidth));
        }

    }
}
