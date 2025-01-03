using UnityEngine;
using DG.Tweening;
using System.Collections;
using Unity.AI.Navigation;


public class DotweenAnimator : MonoBehaviour
{
    private enum MoveType
    {
        Rotate,
        Move,
        Scale,
    }

    [SerializeField] private bool onceOrLoop,backAndForth, saveFinalTransform, resetOnEnable;
    [SerializeField] private MoveType moveType;
    [SerializeField] private Ease easeType;
    [SerializeField] private float animationTime;
    [SerializeField] private float delayTime;
    [Header("Use One Of These:")]
    [SerializeField] private Vector3 animationValue;
    [SerializeField] private Transform endpoint;
    private Vector3 startPos = new();
    private Vector3 startRot = new();
    private Vector3 startScale = new();
    private string id;
    [Header("Optional, Open Original:")]
    [SerializeField] private bool openStatic;
    [SerializeField] private GameObject[] staticObject;
    [SerializeField] private bool NavMeshBaker;
    public NavMeshSurface surface;
    private int modeInt;
    private bool isTweenDone;
    private Tween currentTween; // Store the current tween

    private void Awake()
    {
        id = gameObject.name;
        startPos = transform.position;
        startRot = transform.rotation.eulerAngles;
        startScale = transform.localScale;
        if (onceOrLoop)
            modeInt = -1;
        else
            modeInt = 0;
    }


    private void Start()
    {

        if (saveFinalTransform)
        {
            LoadFinalTransform();
        }
        if (openStatic)
        {
            if (PlayerPrefs.GetInt("OpenStatic" + id) == 1)
            {
                isTweenDone = true;

                foreach (GameObject go in staticObject)
                {
                    go.SetActive(true);
                }
                gameObject.SetActive(false);
            }

        }
    }
    private void OnEnable()
    {
        if (!saveFinalTransform)
        {
            if (!isTweenDone)
            {
                if (moveType == MoveType.Move)
                {
                    if(resetOnEnable)
                        transform.position = startPos;
                    Invoke(nameof(Mover), delayTime);
                }
                if (moveType == MoveType.Rotate)
                {
                    if (resetOnEnable)
                        transform.Rotate(startRot);
                    Invoke(nameof(Rotator), delayTime);
                }
                if (moveType == MoveType.Scale)
                {
                    if (resetOnEnable)
                        transform.localScale = startScale;
                    Invoke(nameof(Scaler), delayTime);
                }
            }
            currentTween.Restart();

        }

    }
    private void OnDisable()
    {
        if (currentTween != null)
        {
            // Stop the current tween when the GameObject is disabled
            currentTween.Kill();
        }


    }
    private void LoadFinalTransform()
    {
        if (moveType == MoveType.Scale)
        {
            if (PlayerPrefs.GetInt("FinalScale" + id, 0) == 1)
            {
                isTweenDone = true;
                if (endpoint == null)
                    gameObject.transform.localScale = startScale + animationValue;
                else
                    gameObject.transform.localScale = endpoint.localScale;
            }
            else
            {
                Invoke(nameof(Scaler), delayTime);
            }

        }
        if (moveType == MoveType.Move)
        {
            if (PlayerPrefs.GetInt("FinalMove" + id, 0) == 1)
            {
                isTweenDone = true;
                if (endpoint == null)
                    gameObject.transform.position = startPos + animationValue;
                else
                    gameObject.transform.position = endpoint.position;
            }
            else
            {
                Invoke(nameof(Mover), delayTime);
            }

        }
        if (moveType == MoveType.Rotate)
        {
            if (PlayerPrefs.GetInt("FinalRotation" + id, 0) == 1)
            {
                isTweenDone = true;
                if (endpoint == null)
                    transform.Rotate(startRot.x + animationValue.x, startRot.y + animationValue.y, startRot.z + animationValue.z);

                else
                    gameObject.transform.rotation = endpoint.rotation;
            }
            else
            {
                Invoke(nameof(Rotator), delayTime);
            }
        }



    }

    private Tween ModeSelector(Vector3 Target)
    {
        Tween tween = null;
        {
            switch (moveType)
            {
                case MoveType.Rotate:
                    tween = transform.DORotate(Target, animationTime).SetEase(easeType);
                    break;

                case MoveType.Move:
                    tween = transform.DOMove(Target, animationTime).SetEase(easeType);
                    break;

                case MoveType.Scale:
                    tween = transform.DOScale(Target, animationTime).SetEase(easeType);
                    break;
            }
        }
        return tween;
    }
    private void Rotator()
    {
        Sequence sequence = DOTween.Sequence();

        if (endpoint != null)
        {
            sequence.Append(ModeSelector(endpoint.rotation.eulerAngles));
            var loopType = LoopType.Incremental;
            if (backAndForth)
            {
                sequence.Append(ModeSelector(startRot));
                loopType = LoopType.Restart;

            }

            sequence.SetLoops(modeInt, loopType);
            
            if (!onceOrLoop)
                sequence.OnComplete(() =>
                {
                    if (saveFinalTransform)
                        PlayerPrefs.SetInt("FinalRotation" + id, 1);

                    sequence.Kill();
                });

        }

        else
        {
            sequence.Append(ModeSelector(transform.rotation.eulerAngles + animationValue));
            var loopType = LoopType.Incremental;
            if (backAndForth)
            {
                sequence.Append(ModeSelector(startRot));
                loopType = LoopType.Restart;

            }


            if (!onceOrLoop)
                sequence.OnComplete(() =>
                {
                    if (saveFinalTransform)
                        PlayerPrefs.SetInt("FinalRotation" + id, 1);

                    sequence.Kill();
                });

            sequence.SetLoops(modeInt, loopType);

        }
        currentTween = sequence;

    }

    private void Mover()
    {
        Sequence sequence = DOTween.Sequence();

        if (endpoint != null)
        {

            sequence.Append(ModeSelector(endpoint.position));
            var loopType = LoopType.Incremental;

            if (backAndForth)
            {
                sequence.Append(ModeSelector(startPos));
                loopType = LoopType.Restart;
            }

            sequence.SetLoops(modeInt, loopType);

            if (!onceOrLoop)
                sequence.OnComplete(() => 
                {
                    if (saveFinalTransform)
                        PlayerPrefs.SetInt("FinalMove" + id, 1);

                    sequence.Kill();
                });

        }

        else
        {
            sequence.Append(ModeSelector(transform.position + animationValue));

            var loopType = LoopType.Incremental;

            if (backAndForth)
            {
                sequence.Append(ModeSelector(startPos));
                loopType = LoopType.Restart;
            }

            sequence.SetLoops(modeInt, loopType);
            if (!onceOrLoop)
                sequence.OnComplete(() =>
                {
                    if (saveFinalTransform)
                        PlayerPrefs.SetInt("FinalMove" + id, 1);

                    sequence.Kill();
                });

        }
        currentTween = sequence;

    }
    private void Scaler()
    {
        Sequence sequence = DOTween.Sequence();

        if (endpoint != null)
        {
            sequence.Append(ModeSelector(endpoint.localScale));

            if (backAndForth)
                sequence.Append(ModeSelector(startScale));

            if (!onceOrLoop)
                sequence.OnComplete(() =>
                {
                    if (saveFinalTransform)
                        PlayerPrefs.SetInt("FinalScale" + id, 1);

                    if (gameObject.activeInHierarchy)
                        StartCoroutine(OpenStaticObjects());

                    if (NavMeshBaker)
                        surface.BuildNavMesh();

                    sequence.Kill();
                });

            sequence.SetLoops(modeInt);

        }
        else
        {
            sequence.Append(ModeSelector(transform.localScale + animationValue));

            if (backAndForth)
                sequence.Append(ModeSelector(startScale));

            if (!onceOrLoop)
                sequence.OnComplete(() =>
                {
                    if (saveFinalTransform)
                        PlayerPrefs.SetInt("FinalScale" + id, 1);

                    if (gameObject.activeInHierarchy)
                        StartCoroutine(OpenStaticObjects());

                    if (NavMeshBaker)
                        surface.BuildNavMesh();

                    sequence.Kill();
                });

            sequence.SetLoops(modeInt);

        }
        currentTween = sequence;

    }
    private IEnumerator OpenStaticObjects()
    {
        if (openStatic)
        {
            PlayerPrefs.SetInt("OpenStatic" + id, 1);
            foreach (var item in staticObject)
            {       
                item.SetActive(true);
            }
            gameObject.SetActive(false);
        }
        yield return null;
    }

    

}

