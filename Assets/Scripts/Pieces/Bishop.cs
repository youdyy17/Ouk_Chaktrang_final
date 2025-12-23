using UnityEngine;
using UnityEngine.UI;

public class Bishop : BasePiece
{
    public override void Setup(Color newTeamColor, Color32 newSpriteColor, PieceManager newPieceManager)
    {
        // Base setup
        base.Setup(newTeamColor, newSpriteColor, newPieceManager);

        // Bishop can move one step in the configured directions
        mMovement = new Vector3Int(0, 1, 1);
        Sprite[] sprites = Resources.LoadAll<Sprite>("W");

        foreach (Sprite s in sprites)
        {
            if (s.name == "White_Bishop")
            {
                GetComponent<Image>().sprite = s;
                break;
            }
        }
    }

    protected override void CheckPathing()
    {
        // Flip direction for black so "forward" is always toward the opponent
        int forward = mColor == Color.white ? 1 : -1;

        CreateCellPath(0, forward, 1);      // forward
        CreateCellPath(-1, forward, 1);     // forward-left
        CreateCellPath(1, forward, 1);      // forward-right
        CreateCellPath(1, -forward, 1);     // backward-right
        CreateCellPath(-1, -forward, 1);    // backward-left
    }
}
