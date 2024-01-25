using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static CreatePiece;
using static BattleManager;
using static PieceMovement;
using static GameManager;
//using static EventManager;



public class Targeting : MonoBehaviour//, IPointerEnterHandler//, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    // This code will be used to manage mouse usage on the grid, for hovering over tiles,
    // selecting tiles as well as drag-dropping pieces
    public Tilemap baseTile;
    public Tilemap clickingTilemap;
    public Tilemap movementTilemap;
    public Tilemap hoveringTilemap;
    public TileBase[] hoverAndClickTiles;
    public TileBase[] movementTiles;

    private Tilemap mainTilemap;

    private Dictionary<BattleManager.TileControl,List<Unit>> occupancy;

    // User input
    private Vector2 clickPos;
    private Vector3Int cellClickPos;

    // Data storage for gameplay
    private Vector3Int prevCellPosHover;
    private Vector3Int prevCellClickPos;
    private Vector3Int prevTileSelectPos;
    private Vector3Int savedAllyCellPos;
    private Vector3Int savedCellPos;

    public List<Vector3Int> moveLocationList = new List<Vector3Int>();
    public List<Vector3Int> savedMoveLocationList = new List<Vector3Int>();
    private List<Move> pieceMoveset;
    private List<Unit> selectedPieceList;
    private Unit pieceToMove;

    // Data storage for efficiency and back-end
    private InputAction.CallbackContext context;
    private Vector2 prevPointerInput;

    // Other code packages
    private BattleManager BM;
    private PieceMovement PM;
    private EventManager eventManager;


    // Interactions using Input System package
    private Vector2 pointerInput;
    public Vector2 PointerInput => pointerInput;

    // Input (cursor select action and position)
    [SerializeField]
    private InputActionReference selectAction, pointerPosition;

    // Variable Initialisation
    //List<Vector3Int> moveLocationList = new List<Vector3Int>();
    //List<Vector3Int> savedMoveLocationList = new List<Vector3Int>();


    // Saved values in process
    //private Vector2 clickPos;


    // Defining which interaction is which tile
    int tileClickSelect = 0;
    int tileClickEmpty = 1;
    int tileHover = 2;
    // Defining tiles for movement illustration
    int tileAvailableMove = 0;

    //public void OnPointerEnter(PointerEventData eventData)
    //{
        
        //Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //    Vector3Int cellPos = hoveringTilemap.WorldToCell(eventData.position);
    //    Debug.Log(cellPos);
    //    hoveringTilemap.SetTile(cellPos, hoverAndClickTiles[tileHover]);
    //}




    //Dictionary<BattleManager.TileControl,List<Unit>> occupancy = new Dictionary<BattleManager.TileControl,List<Unit>>();


    
    private void Awake()
    {
        // Find the Grid GameObject
        GameObject gridObj = GameObject.Find("Grid");

        // Get the BattleManager component from the Grid GameObject
        BM = gridObj.GetComponent<BattleManager>();
        if (BM == null)
        {
            Debug.Log("BattleManager component not found");
            return;
        }

        PM = gridObj.GetComponent<PieceMovement>();
        if (PM == null)
        {
            Debug.Log("PieceMovement component not found");
            return;
        }
        

        GameObject mainTilemapObj = GameObject.Find("Tilemap");
        
        mainTilemap = mainTilemapObj.GetComponent<Tilemap>();

    }

    // Start is called before the first frame update
    void Start()
    {

        // Enabling click action
        //selectAction.action.started += context =>
        //{
        //    if (inBattle){TilePress();}
        //};
        
        //selectAction.action.performed += context => {Debug.Log("Click performed");};
        //selectAction.action.canceled += context => {
        //    AfterClickAction();
        //};
        

        // Enabling right click action
        /* selectAction.action.started += context =>
        {
            OnClickAction();
        };
        selectAction.action.performed += context => {Debug.Log("performed");};
        selectAction.action.canceled += context => {
            AfterClickAction();
        }; */
    }

    // Update is called once per frame
    void Update()
    {
        ////////
        // Using input system package
        pointerInput = GetCursorInput(pointerPosition);

        // Skip all work if cursor not moved
        if (pointerInput == prevPointerInput) 
        {
            return;
        }

        Vector3Int cellPosHover = GetCursorToBoard(pointerInput);
        // Change tile to hover
        if (mainTilemap.HasTile(cellPosHover))
        {
            hoveringTilemap.SetTile(cellPosHover, hoverAndClickTiles[tileHover]);
        }
        // If new cell hovered, old cell cleared
        if (prevCellPosHover != cellPosHover)
        {
            hoveringTilemap.SetTile(prevCellPosHover, null);
        }
        prevCellPosHover = cellPosHover;

        /////////

        
        prevPointerInput = pointerInput;
        
    }

    //////---===---//////
    // Functions
    //////---===---//////

    // Give _pointerPosition get mousePosition
    private Vector2 GetCursorInput(InputActionReference _pointerPosition)
    {
        Vector2 mousePosition = _pointerPosition.action.ReadValue<Vector2>();
        return mousePosition;
    }

    // Give _mousePosition get mouseCellPos
    private Vector3Int GetCursorToBoard(Vector2 _mousePosition)
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(_mousePosition);
        Vector3Int mouseCellPos = hoveringTilemap.WorldToCell(mouseWorldPosition);
        return mouseCellPos;
    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        // Subscribe to the MouseInputProvider events (clicking etc.)
        eventManager = FindObjectOfType<EventManager>();
        if (eventManager != null)
        {
            eventManager.onMouseClicked.AddListener(TilePress);
            eventManager.onMouseReleased.AddListener(AfterClickAction);
        }


        //selectAction.action.Enable();
        //selectAction.action.performed += OnClickAction();
        //EventManager.OnClickAction += TilePress;

        // Enabling click action
        //selectAction.action.started += context =>
        //{
        //    OnClickAction();
        //};
        //selectAction.action.started += context =>
        //{
        //    int i=1;
        //    TilePress();
        //};
        
        //selectAction.action.performed += context => {Debug.Log("Click performed");};
        //selectAction.action.canceled += context => {AfterClickAction();};

    }

    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    void OnDisable()
    {
        if (eventManager != null)
        {
            eventManager.onMouseClicked.RemoveListener(TilePress);
            eventManager.onMouseReleased.RemoveListener(AfterClickAction);
        }


        //EventManager.OnClickAction -= 
    }

    // On click action,
    public void TilePress()
    {
        if(gamePaused){return;}
        if(!inBattle){return;}

        // Select Piece First

        // Get clicking position on screen
        clickPos = GetCursorInput(pointerPosition);
        // Where clicked on board
        cellClickPos = GetCursorToBoard(clickPos);

        // If no tile, return
        if (!mainTilemap.HasTile(cellClickPos))
        {
            PieceDeselect(savedAllyCellPos);

            return;
        }


        // Change tile sprite based on where clicked
        occupancy = BM.OccupiedBy(new Vector2Int(cellClickPos.x, cellClickPos.y));

        if(pieceToMove != null)
        {
            PM.MovePieceTo(pieceToMove, cellClickPos);
        }


        if (occupancy.ContainsKey(TileControl.Ally) || occupancy.ContainsKey(TileControl.Contested))
        {
            
            PieceSelect(cellClickPos);

        }
        else if (occupancy.ContainsKey(TileControl.Enemy) || occupancy.ContainsKey(TileControl.Empty)) // Clicking on empty or enemy tile
        {
            savedCellPos = cellClickPos;
            clickingTilemap.SetTile(cellClickPos, hoverAndClickTiles[tileClickEmpty]);

            //if (context.canceled) 
            //{
            //    TileClear(cellClickPos, clickingTilemap);
            //}

            prevTileSelectPos = cellClickPos; //Store later for guard function

            PieceDeselect(savedAllyCellPos);
        }
        

        

    }


    // Actions taken after click
    public void AfterClickAction()
    {   
        // If click over (unclick) clear enemy/empty tile info/sprite
        TileClear(prevTileSelectPos, clickingTilemap);
    }

    // De-selecting piece
    // TO PUT CODE HERE

    // Pick piece on select (default click), ally pieces and show movement (and other?) options
    public void PieceSelect(Vector3Int cellClickPos)
    {
        // Change tile
        clickingTilemap.SetTile(cellClickPos, hoverAndClickTiles[tileClickSelect]);
        if (cellClickPos != savedAllyCellPos)
        {
            // De-select previous piece/tile
            Debug.Log("Unmarking: " + savedAllyCellPos);
            PieceDeselect(savedAllyCellPos);
        }
        moveLocationList.Clear(); // Clear unsaved prev click data

        // Select piece and show moves
        selectedPieceList = BM.GetUnitsFromTile(cellClickPos);
        

        foreach (Unit selectedPiece in selectedPieceList)
        {
            selectedPiece.isSelected = true;
            pieceToMove = selectedPiece;
            pieceMoveset = PM.SeekMovesOf(selectedPiece);

            foreach (Move individualMove in pieceMoveset)
            {
                Vector3Int moveLocation = new Vector3Int (individualMove.TargetSquare.x, individualMove.TargetSquare.y,0);
                moveLocationList.Add(moveLocation);

                // Mark tiles
                movementTilemap.SetTile(moveLocation, movementTiles[tileAvailableMove]);
            }
            savedMoveLocationList.AddRange(moveLocationList); // Store for removing previous marks

        }

        // If another piece is selected or rclick (default), then unselect, prev tile set null

        // TO ADD CODE
        //clickingTilemap.SetTile(cellClickPos, null);


        savedAllyCellPos = cellClickPos; //Store later for update
    }

    // Unmarking tile (whatever it is)
    public void TileClear(Vector3Int clearPos, Tilemap clearTilemap)
    {
        clearTilemap.SetTile(clearPos, null);
        
    }

    // Unmarking tile (whatever it is), clearing pick storage
    public void PieceDeselect(Vector3Int cellPos)
    {
        // Remove visual (same as unmarked)
        clickingTilemap.SetTile(cellPos, null);

        // Get piece from location
        selectedPieceList = BM.GetUnitsFromTile(cellPos);


        // Remove previous potential move marked tiles (and its list)
        foreach (Vector3Int savedMoveLocation in savedMoveLocationList)
        {
            TileClear(savedMoveLocation, movementTilemap);
        }
        savedMoveLocationList.Clear();

        // Mark as unselected
        foreach (Unit selectedPiece in selectedPieceList)
        {
            selectedPiece.isSelected = false;
        }
    }


    // Clicking on empty/enemy tile //DEPRECATED: //TODO: REMOVE
    void TilePress(Vector3Int cellClickPos)
    {
        // Clicking on empty or enemy tile
        savedCellPos = cellClickPos;
        clickingTilemap.SetTile(cellClickPos, hoverAndClickTiles[tileClickEmpty]);

        if (context.canceled) 
        {
            TileClear(cellClickPos, clickingTilemap);
        }

        prevTileSelectPos = cellClickPos; //Store later for guard function
    }


}
