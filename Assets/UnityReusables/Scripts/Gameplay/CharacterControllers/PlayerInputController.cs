using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityReusables.CharacterControllers
{
    public class PlayerInputController : MonoBehaviour
    {
        public BaseCharacterController controller;
        public bool fakeMove;

        private float _horizontalMove;
        private bool _jump;
        private bool _crouch;

        private void Update()
        {
            if (!fakeMove)
                _horizontalMove = Input.GetAxis("Horizontal");

            if (Input.GetButtonDown("Jump"))
            {
                _jump = true;
            }

            if (Input.GetButtonDown("Crouch"))
            {
                _crouch = true;
            }
            else if (Input.GetButtonUp("Crouch"))
            {
                _crouch = false;
            }
        }

        [Button("Move for seconds")]
        public void MoveForSeconds(float s)
        {
            StartCoroutine(FakeMove(s));
        }

        private IEnumerator FakeMove(float s)
        {
            _horizontalMove = 1f;
            yield return new WaitForSeconds(s);
            _horizontalMove = 0f;
        }

        private void FixedUpdate()
        {
            controller.Move(_horizontalMove, _crouch, _jump);
            _jump = false;
        }
    }
}