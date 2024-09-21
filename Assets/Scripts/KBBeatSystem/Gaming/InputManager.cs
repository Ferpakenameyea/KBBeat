using KBBeat.Scripts.Inputs;
using KBBeat.Common;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

internal class InputManager : MonoBehaviour
{
    [SerializeField] private string inputType = "Android";
    public static int LeftHitCount { get; private set; }
    public static int LeftHoldCount { get; private set; }
    public static int RightHitCount { get; private set; }
    public static int RightHoldCount { get; private set; }

    public static InputManager Instance { get; private set; } = null;
    internal InputSource inputSource = null;

    public string InputType { get => inputType; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InputSourceAttacher.Attach(this, this.InputType);
            this.transform.parent = null;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (BuiltInSettings.GamePaused)
        {
            return;
        }
        this.inputSource.GetAll(out int leftHit, out int leftHold, out int rightHit, out int rightHold);
        LeftHitCount = leftHit;
        RightHitCount = rightHit;
        LeftHoldCount = leftHold;
        RightHoldCount = rightHold;
    }

    public static int GetHitCount(Directions direction)
    {
        if (Instance == null)
        {
            Debug.LogError($"Trying to access an unlaunched input manager!");
        }
        return direction switch
        {
            Directions.LEFT => LeftHitCount,
            Directions.RIGHT => RightHitCount,
            _ => -1
        };
    }

    public static int GetHoldCount(Directions direction)
    {
        if (Instance == null)
        {
            Debug.LogError($"Trying to access an unlaunched input manager!");
        }
        return direction switch
        {
            Directions.LEFT => LeftHoldCount,
            Directions.RIGHT => RightHoldCount,
            _ => -1
        };
    }
}

namespace KBBeat.Scripts.Inputs
{

    [Serializable]
    public struct KeySets
    {
        [SerializeField] public List<KeyCode> LeftKeys;
        [SerializeField] public List<KeyCode> RightKeys;

        public KeySets(List<KeyCode> left, List<KeyCode> right)
        {
            this.LeftKeys = new(left);
            this.RightKeys = new(right);
        }

        public static readonly KeySets DefaultKeyboardSets = new(
            new() {
            KeyCode.A,
            KeyCode.S,
            KeyCode.D,
            KeyCode.Q,
            KeyCode.W,
            KeyCode.E,
            KeyCode.Z,
            KeyCode.X,
            KeyCode.C,
            KeyCode.V,
            KeyCode.F,
            KeyCode.G
            },
            new() {
            KeyCode.U,
            KeyCode.I,
            KeyCode.O,
            KeyCode.J,
            KeyCode.K,
            KeyCode.L,
            KeyCode.M,
            KeyCode.Comma,
            KeyCode.Period,
            KeyCode.N,
            KeyCode.H,
            KeyCode.Y
            }
        );
    }
    public class KeyBoardInputSource : InputSource
    {
        [SerializeField] internal KeySets keySets = KeySets.DefaultKeyboardSets;

        private void OnDestroy()
        {
            if (InputManager.Instance.inputSource == this)
            {
                InputManager.Instance.inputSource = null;
            }
        }

        private void GetCount(List<KeyCode> keys, out int hitCount, out int holdCount)
        {
            var hitRes = 0;
            var holdRes = 0;
            foreach (var key in keys)
            {
                if (Input.GetKeyDown(key))
                {
                    hitRes++;
                }
                if (Input.GetKey(key))
                {
                    holdRes++;
                }
            }
            hitCount = hitRes;
            holdCount = holdRes;
        }

        public override void GetAll(
            out int leftHitCount, out int leftHoldCount,
            out int rightHitCount, out int rightHoldCount
            )
        {
            this.GetCount(keySets.LeftKeys, out int leftHit, out int leftHold);
            this.GetCount(keySets.RightKeys, out int rightHit, out int rightHold);

            leftHitCount = leftHit;
            leftHoldCount = leftHold;
            rightHitCount = rightHit;
            rightHoldCount = rightHold;

            return;
        }
    }
    public class AndroidInputSource : InputSource
    {
        private static float border;

        private void Start()
        {
            border = Camera.main.pixelWidth / 2f;
        }

        public override void GetAll(out int leftHitCount, out int leftHoldCount, out int rightHitCount, out int rightHoldCount)
        {
            int leftHit = 0, rightHit = 0;
            int leftHold = 0, rightHold = 0;
            foreach (var hit in Input.touches)
            {
                var direction = hit.position.x > border ? Directions.RIGHT : Directions.LEFT;

                if (hit.phase == TouchPhase.Began)
                {
                    if (direction == Directions.RIGHT)
                    {
                        rightHit++;
                    }
                    else
                    {
                        leftHit++;
                    }
                }

                if (direction == Directions.LEFT)
                {
                    leftHold++;
                }
                else
                {
                    rightHold++;
                }
            }
            leftHitCount = leftHit;
            rightHitCount = rightHit;
            leftHoldCount = leftHold;
            rightHoldCount = rightHold;
        }

        private void Awake()
        {
            Input.multiTouchEnabled = true;
        }
    }
    internal static class InputSourceAttacher
    {
        public static void Attach(InputManager manager, string type)
        {
            if (manager.GetComponent<InputSource>() != null)
            {
                return;
            }

            switch (type)
            {
                case "KeyBoard":
                    manager.AddComponent<KeyBoardInputSource>();
                    break;
                case "Android":
                    manager.AddComponent<AndroidInputSource>();
                    break;
                default:
                    Debug.LogError($"unknown input type: {type}");
                    break;
            }
            manager.inputSource = manager.GetComponent<InputSource>();
        }
    }
    public abstract class InputSource : MonoBehaviour
    {
        public abstract void GetAll(
            out int leftHitCount, out int leftHoldCount,
            out int rightHitCount, out int rightHoldCount
            );
    }
}