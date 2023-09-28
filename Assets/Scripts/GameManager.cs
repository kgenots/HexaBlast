using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject winScreen;
    public GameObject loseScreen;
    // public Button loseRestartButton;
    // public Button winRestartButton;
    // public Button restartButton;
    public TextMeshProUGUI targetsText;
    public TextMeshProUGUI movesText;

    private HexGrid grid;
    private EffectManager effectManager;
    private ElementType selectedElementType;
    private Transform selectedElementTransform;
    //private List<IntVector2> selectedElementIndices;
    private IntVector2 selectedElement;
    private IntVector2 lastGridIndexToHoverOver;
    private Vector3 lastMousePos;

    private float swapMovementSpeedIncrementMultiplier = 8f;

    private int boxes;
    private int moves;

    public bool IsGameOver { get; private set; }

    private void Start()
    {
        grid = GetComponent<HexGrid>();
        effectManager = GetComponent<EffectManager>();
        selectedElement = IntVector2.NullVector;
        lastGridIndexToHoverOver = IntVector2.NullVector;
        lastMousePos = Vector3.zero;

        // restartButton.onClick.AddListener(OnRestartButtonPressed);
        // loseRestartButton.onClick.AddListener(OnRestartButtonPressed);
        // winRestartButton.onClick.AddListener(OnRestartButtonPressed);

        grid.OnAutoMatchesFound -= AutoMatchCallback;
        grid.OnAutoMatchesFound += AutoMatchCallback;

        grid.OnDestroyWood -= CheckCountBoxes;
        grid.OnDestroyWood += CheckCountBoxes;

        grid.OnSuccessMoves -= CheckCountMoves;
        grid.OnSuccessMoves += CheckCountMoves;

        effectManager.OnPointPopupEffectFinished -= PointPopupEffectFinishCallback;
        effectManager.OnPointPopupEffectFinished += PointPopupEffectFinishCallback;

        ResetCounters();
        GameStart();
    }

    private void Update()
    {
        if (IsGameOver) return;

        #region LeftMouseButton Down
        if (Input.GetMouseButtonDown(0) && grid.GetIsElementMovementDone())
        {
            var hitGridIndex = grid.GetGridIndexFromWorldPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (hitGridIndex.x >= 0 && hitGridIndex.y >= 0)
            {
                selectedElement = hitGridIndex;
                var elementData = grid.GetGridElementDataFromIndex(selectedElement);
                selectedElementType = elementData.elementType;
                if (selectedElementType != null && grid.IsNotTargetType(selectedElementType))
                {
                    selectedElementTransform = elementData.elementTransform;
                    effectManager.SpawnSelectionEffectAtIndex(hitGridIndex);
                }
            }
            lastGridIndexToHoverOver = hitGridIndex;
        }
        #endregion

        #region LeftMouseButton Held
        if (Input.GetMouseButton(0) && selectedElement != IntVector2.NullVector)
        {
            if (lastMousePos != Input.mousePosition)
            {
                var hitGridIndex = grid.GetGridIndexFromWorldPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                if (lastGridIndexToHoverOver != hitGridIndex)
                {
                    if (hitGridIndex != IntVector2.NullVector)
                    {
                        if (grid.CheckIfNeighbours(selectedElement, hitGridIndex))
                        {
                            var hitElementType = grid.GetGridElementDataFromIndex(hitGridIndex).elementType;
                            if (hitElementType != null && grid.IsNotTargetType(hitElementType))
                            {
                                effectManager.SpawnSelectionEffectAtIndex(hitGridIndex);
                            }
                        }
                    }

                    if ((lastGridIndexToHoverOver.x >= 0 && lastGridIndexToHoverOver.y >= 0) && lastGridIndexToHoverOver != selectedElement)
                        effectManager.ClearSelectionEffectAtIndex(lastGridIndexToHoverOver);
                    lastGridIndexToHoverOver = hitGridIndex;
                }

                lastMousePos = Input.mousePosition;
            }
        }
        #endregion

        #region LeftMouseButton Up
        if (Input.GetMouseButtonUp(0) && selectedElementTransform != null)
        {
            var hitGridIndex = grid.GetGridIndexFromWorldPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (hitGridIndex != IntVector2.NullVector)
            {
                if (grid.CheckIfNeighbours(selectedElement, hitGridIndex))
                {
                    var hitElementType = grid.GetGridElementDataFromIndex(hitGridIndex).elementType;
                    if (hitElementType != null && grid.IsNotTargetType(hitElementType))
                    {
                        var releasePointIndex = hitGridIndex;

                        grid.SwapElementsRecord(selectedElement, releasePointIndex);
                        grid.MoveElementsToCorrectPositions(swapMovementSpeedIncrementMultiplier);
                    }
                }
            }

            selectedElement = IntVector2.NullVector;
            selectedElementTransform = null;
            effectManager.ClearAllSelectionEffects();
        }
        #endregion
    }

    private void ClearSelectionsAndRelatedEffects()
    {
        selectedElement = IntVector2.NullVector;
        selectedElementTransform = null;
        effectManager.ClearSelectionLine();
        effectManager.ClearAllSelectionEffects();
        effectManager.ClearHighlights();
    }

    private void InvalidateSelection()
    {
        effectManager.ClearAllSelectionEffects();
        effectManager.InvalidateSelectionLine();
    }

    private void OnRestartButtonPressed()
    {
        grid.Restart();
        effectManager.Restart();
        ResetCounters();
        GameStart();
    }

    private void ResetCounters()
    {
        boxes = 10;
        UpdateBoxes(boxes);
        moves = 20;
        UpdateMoves(moves);
    }

    private void AddToScore(int scoreToAdd)
    {
        //boxes += scoreToAdd;
        //targetsText.text = "BOXES: " + boxes.ToString();
    }

    private void CheckCountMoves(int value)
    {
        moves += value;
        if (moves <= 0)
            GameOver(false);
        UpdateMoves(moves);
    }

    private void CheckCountBoxes(int value)
    {
        boxes += value;
        if (boxes <= 0)
            GameOver(true);
        UpdateBoxes(boxes);
    }

    private void GameStart()
    {
        winScreen.SetActive(false);
        loseScreen.SetActive(false);

        IsGameOver = false;
    }

    private void GameOver(bool win)
    {
        if (win && !loseScreen.activeInHierarchy)
            winScreen.SetActive(true);
        else if (!win && !winScreen.activeInHierarchy)
            loseScreen.SetActive(true);

        IsGameOver = true;
    }

    private void UpdateMoves(int value)
    {
        movesText.text = $"MOVES: {value}";
    }

    private void UpdateBoxes(int value)
    {
        targetsText.text = $"TARGETS: {value}";
    }

    public void AutoMatchCallback(List<List<IntVector2>> matches)
    {
        //int scoreToAdd = 0;
        //for (int i = 0; i < matches.Count; i++)
        //{
        //	//Count score
        //	int scoreFromMatch = matches[i].Count * matches[i].Count;
        //	scoreToAdd += scoreFromMatch;

        //	//Call effects
        //	effectManager.SpawnPointPopUpsForMatch(matches[i]);
        //}

        //scoreShouldBe += scoreToAdd;
    }

    public void PointPopupEffectFinishCallback(int pointsToAdd)
    {
        AddToScore(pointsToAdd);
    }
}
