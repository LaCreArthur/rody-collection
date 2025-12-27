using Cinemachine;
using UnityEngine;
using UnityReusables.ScriptableObjects.Events;
using UnityReusables.ScriptableObjects.Variables;
using UnityReusables.Utils.LayerTagDropdown;

namespace UnityReusables.PlayerController
{
    public class KinematicDragTouchController : TouchControl
    {
        [Header("Dragging")]
        public float mMoveSpeed = 5.0f;
        public float smoothTime = .3f;
        public float minDragDistance = 0.1f;
        public Vector3 dragScale = new Vector3(1.2f, 2, 1.2f);
        
        [Header("Bloc Tags")]
        [TagDropdown] public string objectiveTag;
        [TagDropdown] public string destructibleTag;
        [TagDropdown] public string staticTag;
        [TagDropdown] public string explosiveTag;
        public LayerMask physicObjectLayer;
        
        [Header("Explosion force")]
        public float efRadius = 5;
        public float minEfPower = 50;
        public float maxEfPower = 300;
        public float maxEfDistToDestructBlocks = 3;
        
        [Header("Touching")]
        public float touchRadius = 5;
        public float minVisiblePercent = 75;
        public Transform[] visibilityCheckers;
        
        [Header("Objective")]
        public TransformVariable objectiveSO;
        public SimpleEventSO levelCompleted;
        public SimpleEventSO gameOver;
        public BoolVariable tutoPhase1Completed, tutoPhase2Completed;

        [Header("Feedbacks")]
        public VibrationManagerSO vibrationManager;

        Transform selectedObject;
        Camera _cam;
        Plane _objPlane;
        Vector3 _startTouchPos, _touchPoint, _desiredPos, _velocity;
        float _log, _fVel;
        bool _isDragging, _isTouchingObject;
        Collider[] results = new Collider[256];

        protected void Start()
        {
            _touchPoint = transform.position;
            _cam = Camera.main;
            _objPlane = new Plane(Vector3.up, _touchPoint);
            transform.localScale = Vector3.zero;
        }

        Vector3 GetTouchPointOnPlane()
        {
            // get touch position on plane
            Ray mRay = _cam.ScreenPointToRay(Input.mousePosition);
            if (_objPlane.Raycast(mRay, out float rayDist))
                return mRay.GetPoint(rayDist);
            else
            {
                Debug.LogWarning("plane raycast failed, returning vector zero", this);
                return Vector3.zero;
            }
        }

        protected override void OnTouchDown()
        {
            _startTouchPos  = GetTouchPointOnPlane();
            CheckSelection();
        }

        protected override void OnTouchHold() { }

        void DestructObject(Transform obj, bool fromTNT = false)
        {
            _isTouchingObject = false; // avoid reraising event

            if (obj.CompareTag(objectiveTag))
            {
                if (fromTNT)
                    gameOver.Raise();
                else
                    levelCompleted.Raise();
                
                OnDestructObject(obj);
            }
            else if (obj.CompareTag(destructibleTag) || (fromTNT && obj.CompareTag(staticTag)))
            {
                // small explosion to awake nearby RB
                AddExplosionForce(obj, efRadius / 2, maxEfPower / 2, obj.position, false);
                vibrationManager.Haptic(HapticTypes.MediumImpact);
                OnDestructObject(obj);
            }
            else if (obj.CompareTag(explosiveTag))
            {
                // screen shake
                GetComponent<CinemachineImpulseSource>().GenerateImpulse(10f);
                vibrationManager.Haptic(HapticTypes.HeavyImpact);
                AddExplosionForce(obj, efRadius, maxEfPower * 10, obj.position, true);
                OnDestructObject(obj);
            }
        }

        public void OnTNTExplode(Transform tnt) => DestructObject(tnt, true);

        void OnDestructObject(Transform obj)
        {
        }

        protected override void OnTouchUp() => selectedObject = null;

        public void DoTouchUp() => OnTouchUp();

        Transform previouslyTouchedObject;
        
        void CheckSelection()
        {
            _isTouchingObject = false;
            var camPos = _cam.transform.position;
            // raycast cam to touch point to check what object is touched
            if (Physics.Raycast(camPos, _startTouchPos - camPos, out var hit, 50f, physicObjectLayer))
            {
                selectedObject = hit.transform;
                //Debug.Log($"touched {selectedObject.name}");
                
                // set checkers for the visibility percentage
                SetVisibilityCheckerPos(selectedObject);
                // check if object is visible enough to be selected
                _isTouchingObject = GetVisibleChecker() > minVisiblePercent;
            }

            if (_isTouchingObject)
            {
                // if touching static, screen shake and that's it
                if (selectedObject.CompareTag(staticTag))
                {
                    // FOR TUTO : after phase 1 (completing two first level, phase 2 is to click on a static block to show the swipe anim
                    if (tutoPhase1Completed.v || !tutoPhase2Completed.v)
                        tutoPhase2Completed.v = true;
                    
                    GetComponent<CinemachineImpulseSource>().GenerateImpulse(5f);
                    return;
                }
                
                if (previouslyTouchedObject != selectedObject)
                {
                    previouslyTouchedObject = selectedObject;
                    vibrationManager.Haptic(HapticTypes.SoftImpact);
                }
                else
                {
                    
                }
            }
        }

        float GetVisibleChecker()
        {
            int visibleChecker = 0;
            for (int i = 0; i < visibilityCheckers.Length; i++)
            {
                Vector3 checkerPos = visibilityCheckers[i].position;
                // check if object is in line of sight with camera 
                if (Physics.Raycast(checkerPos, _cam.transform.position - checkerPos, out var hit))
                {
                    if (hit.transform == _cam.transform)
                        visibleChecker++;
                }
            }

            float visiblePercent = ((float) visibleChecker / visibilityCheckers.Length) * 100f;
            //Debug.Log($"visibility percentage = {visiblePercent}");
            return visiblePercent;
        }

        void SetVisibilityCheckerPos(Transform newTarget)
        {
            if (visibilityCheckers.Length <= 0) return;
            var parent = visibilityCheckers[0].parent;
            parent.position = newTarget.position;
        }
        
        void FixedUpdate()
        {
            if (!_isDragging) return;
            _desiredPos = Vector3.Lerp(_desiredPos,_touchPoint, Time.fixedDeltaTime * mMoveSpeed);
            transform.position = Vector3.SmoothDamp(transform.position, _desiredPos, ref _velocity, smoothTime);
        }

        public void AddExplosionForce(Transform objSource, float radius, float power, Vector3 explosionPos, bool fromTNT)
        {
            var size = Physics.OverlapSphereNonAlloc(explosionPos, radius, results);
            for (int i = 0; i < size; i++)
            {
                var hit = results[i];
                Rigidbody rb = hit.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.AddExplosionForce(power, explosionPos, radius, 3.0f);
                    // if fromTNT : destroy all nearby objects, even statics, if goal is destroyed : gameover 
                    if (fromTNT && Vector3.SqrMagnitude(objSource.position - hit.transform.position) 
                                        < maxEfDistToDestructBlocks)
                    {
                        // check if explosion touches another TNT
                        DestructObject(hit.transform, true);
                    }
                }
            }
        }
    }
}