using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlashMouv : MonoBehaviour
{
    
    enum StatePlayer { IDLE, Run, Walk, Crounch }
    [SerializeField] InputActionReference _moveInput;
    [SerializeField] Transform _root;
    [SerializeField] float _speedMin;
    [SerializeField] float _MovingThreshold;
    [SerializeField] Animator _animator;
    Vector3 _playerMovement;
    [SerializeField] GameObject _playerCamera;
    // [SerializeField] Rigidbody rb;
    [SerializeField] CharacterController _chara;
    [SerializeField] float _gravityPush;

    [SerializeField] InputActionReference _jumpInput;

    [SerializeField] float _jumpPower;
    [SerializeField] float _jumpMax; //saut max
    [SerializeField] float _jumpNumbercurrent; // mon nombre de saut
    [SerializeField] float _minJump;
    [SerializeField] bool _isJumping;

    /*[SerializeField] InputActionReference _crounchInput;

    [SerializeField] InputActionReference _ArmeWeapon;
*/
    [SerializeField] InputActionReference _SprintInput;

    [SerializeField] float _sprint;
    Vector3 _gravity;

    private float _currentSpeed;

    StatePlayer _playerState;
    private float _time;

    [SerializeField] AnimationCurve _SprintCurve;

    [SerializeField] float _MaxSpeed;
    [SerializeField] float _sprintTIme;
    [SerializeField] AudioSource _audio;
    [SerializeField] GameObject _effect;
    void Start()
    {
        _currentSpeed = _speedMin;

        _playerState = StatePlayer.IDLE;
        _moveInput.action.started += UdpateMove;
        _moveInput.action.performed += UdpateMove;
        _moveInput.action.canceled += EndMove;

        /* _jumpInput.action.started += StartJump;*/

        /*      _crounchInput.action.started += StartCrounch;
              _crounchInput.action.performed += UdpateCrounch;
              _crounchInput.action.canceled += EndCrounch;*/


        _SprintInput.action.started += StartSprint;
        _SprintInput.action.performed += uptadeSprint;
        _SprintInput.action.canceled += CancelSprint; ;

    }

    private void Update()
    {
        _animator.SetBool("isRunning", _playerState == StatePlayer.Run);
        _animator.SetBool("Iswalking", _playerState == StatePlayer.Walk);
        _animator.SetBool("Crounch", _playerState == StatePlayer.Crounch);

        if (_playerMovement.magnitude > _MovingThreshold)  // si on est ent train de bouger alors 
        {
            _animator.SetFloat("Horrizontal", _playerMovement.x);
            _animator.SetFloat("Vertical", _playerMovement.z);
        }
        else
        {
            _animator.SetFloat("Horrizontal", 0);
            _animator.SetFloat("Vertical", 0);
        }
        if (_playerState == StatePlayer.Walk)
        {
            _currentSpeed = _speedMin;
        }
        
        
        if (_playerState == StatePlayer.Run)
        {
            _time += Time.deltaTime;
            _sprint = _SprintCurve.Evaluate(_time);
            _effect.SetActive(true);
             
        }
        
        else
        {
            _currentSpeed = _speedMin;
            _effect.SetActive(false);
            _time = 0;
            _sprint = 0;
        }
       

    }




    void FixedUpdate()
    {
        // Orientation
        var tmp = new Vector3(_playerCamera.transform.forward.x, 0, _playerCamera.transform.forward.z);
        //  rb.transform.LookAt(rb.transform.position + tmp);
        _chara.transform.LookAt(_chara.transform.position + tmp);


        // Move
        var cameraDirection = _playerCamera.transform.TransformDirection(_playerMovement);
        cameraDirection.y = 0;

        // Gravité
        //_gravity = new Vector3(0, _gravityPush, 0);

        if (_chara.isGrounded)
        {
            if (_isJumping)
            {

            }
            else
            {
                _gravity.y = 0;
                _jumpNumbercurrent = 0;
            }
            //_gravityPush = -7;
        }
        else
        {
            _gravity.y += _gravityPush;
        }
        if (_isJumping && _chara.isGrounded)
        {
            _isJumping = false;
            _animator.SetTrigger("Landing");

        }

        //if (_isJumping)
        //{
        //    _gravityPush = -2;
        //}


        _chara.Move((cameraDirection * _currentSpeed * Time.fixedDeltaTime) + (_gravity * Time.deltaTime));
        if (_playerState == StatePlayer.Run)
        {
            _chara.Move((cameraDirection * _currentSpeed * _sprint * Time.fixedDeltaTime) + (_gravity * Time.deltaTime));
        }


    }

    private void UdpateMove(InputAction.CallbackContext obj)
    {
        Debug.Log("Update Move");
        var joystick = obj.ReadValue<Vector2>();
        _playerMovement = new Vector3(joystick.x, 0, joystick.y);
        if (_playerState == StatePlayer.Run || _playerState == StatePlayer.Crounch)
        {
            // nothing
        }
        else
        {
            _playerState = StatePlayer.Walk;
        }
    }
    void EndMove(InputAction.CallbackContext obj)
    {
        if (_playerState == StatePlayer.Walk)
        {
            _playerState = StatePlayer.IDLE;
        }
        _playerMovement = new Vector3(0, 0, 0);
    }
    /*  private void StartJump(InputAction.CallbackContext obj)
      {
          if (_chara.isGrounded && _jumpNumbercurrent < _jumpMax)
          {
              _gravity = new Vector3(0, _jumpPower, 0);
              _animator.SetTrigger("Jump");
              //_chara.Move (_gravity * Time.fixedDeltaTime);
              _isJumping = true;
              _jumpNumbercurrent++;
          }
      }*/
    /*    public void StartCrounch(InputAction.CallbackContext obj)
        {
            _playerState = StatePlayer.Crounch;
        }
        private void UdpateCrounch(InputAction.CallbackContext obj)
        {
            _playerState = StatePlayer.Crounch;
        }
        void EndCrounch(InputAction.CallbackContext obj)
        {
            if (_playerMovement.magnitude > 0.1f)
            {
                _playerState = StatePlayer.Walk;
            }
            else
            {
                _playerState = StatePlayer.IDLE;
            }
        }*/



    private void StartSprint(InputAction.CallbackContext obj)
    {
        _audio.Play();
        _playerState = StatePlayer.Run;
        
        StartCoroutine(AddSprint());
    }
    private void uptadeSprint(InputAction.CallbackContext obj)
    {
        Debug.Log("Update Sprint");
        _playerState = StatePlayer.Run;
    }
    private void CancelSprint(InputAction.CallbackContext obj)
    {
        if (_playerMovement.magnitude > 0.1f)
        {
            _playerState = StatePlayer.Walk;
        }
        else
        {
            _playerState = StatePlayer.IDLE;
        }
        StopCoroutine(AddSprint());
        _audio.Stop();
    }

    private void OnValidate()
    {
        _SprintCurve.MoveKey(_SprintCurve.keys.Length - 1, new Keyframe(_sprintTIme, _MaxSpeed));
    }

    IEnumerator AddSprint()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (_sprint >= _MaxSpeed)
            {
                _currentSpeed++;
            }
        }
    }
}
