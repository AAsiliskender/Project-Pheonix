using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static CreatePiece;
//using static ArmyManager;

public class PieceManager : MonoBehaviour
{
    private GameObject placementObj; 
    private GameObject placementOnOrOffField;
    
    // Add piece to field of battle
    public void AddPiece(Unit piece,  Vector2Int pos)//int UnitId, int UnitClass, int Faction, int[] pos)
        {

            // Change Unit position to what it is on map
            piece.Position = pos;
            //if (piece.Faction == Affiliation.Ally)
            //{
            //    UpdateUnit(ArmyManager.AllyUnitList, ArmyManager.AllyUnitDict, piece.UnitId, piece);
            //} else if (piece.Faction == Affiliation.Enemy)
            //{
            //    UpdateUnit(ArmyManager.EnemyUnitList, ArmyManager.EnemyUnitDict, piece.UnitId, piece);
            //}

            // Obtain GameObect Player's army and we will add pieces to it
            GameObject GameManagerObj = GameObject.Find("GameManager"); //GENERALISE THIS
            ArmyManager AM = GameManagerObj.GetComponent<ArmyManager>();



            // Change angle and position of the piece's GameObject as needed
            piece.ThisGameObject.transform.position = new Vector3((float)(pos.x) + 0.5f, 0.01f, (float)(pos.y) + 0.5f);
            piece.ThisGameObject.transform.eulerAngles = new Vector3(90f,0f,0f);

            // Set unit to be in action
            piece.IsOutOfAction = false;


            // First, get references to the armies and the sub GameObjects (down to On_Field and Off_Field)
            // Finding the GameObject based on host faction affiliation
            if (piece.AttachedArmy.Faction == Affiliation.Player)
            {
                placementObj = GameObject.Find("Player").gameObject;
            } else if (piece.AttachedArmy.Faction == Affiliation.Ally)
            {
                placementObj = GameObject.Find("Ally").gameObject;
            } else if (piece.AttachedArmy.Faction == Affiliation.Enemy)
            {
                placementObj = GameObject.Find("Enemy").gameObject;
            }
            GameObject placementArmyObj = GameObject.Find(piece.AttachedArmy.ArmyName).gameObject; // "Player Army #1"
            GameObject placementPiecesObj = placementArmyObj.transform.Find("Pieces").gameObject;
            // Place into relevant GameObject child (On_Field or Off_Field)
            if (!piece.IsOutOfAction)
            {
                placementOnOrOffField = placementPiecesObj.transform.Find("On_Field").gameObject;
            } else {
                placementOnOrOffField = placementPiecesObj.transform.Find("Off_Field").gameObject;
            }
            piece.ThisGameObject.transform.SetParent(placementOnOrOffField.transform);

            /////////////////////
            // Call renderer to add relevant sprite
            PieceRenderer PR = GetComponent<PieceRenderer>();
            PR.RenderPiece(piece.UnitClass, piece.AttachedArmy.Faction, piece.ThisGameObject);
            

            return;
        }

    // Update unit
    public void UpdateUnit(List<Unit> unitList, Dictionary<int, Unit> unitDict, int unitIDtoChange, Unit updatedUnit)
    {
        int indexToReplace = unitList.FindIndex(u => u.UnitId == unitIDtoChange);

        // Updating Unit on unit list
        if (indexToReplace != -1)
        {
            unitList[indexToReplace] = updatedUnit;
        }

        // Updating Unit on unit dictionary
        if (unitDict.ContainsKey(unitIDtoChange))
        {
            unitDict[unitIDtoChange] = updatedUnit;
        }

        //Debug.Log("Placed unit: " + unitList[indexToReplace].UnitName);
    }
}
