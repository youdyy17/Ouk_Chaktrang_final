using UnityEngine;
using UnityEngine.UI;

public class Pawn : BasePiece
{
    public override void Setup(Color newTeamColor, Color32 newSpriteColor, PieceManager newPieceManager)
    {
        // Base setup
        base.Setup(newTeamColor, newSpriteColor, newPieceManager);

        // Pawn Stuff
        // Pawn movement: y controls forward step, z controls diagonal direction/sign
        mMovement = mColor == Color.white ? new Vector3Int(0, 1, 1) : new Vector3Int(0, -1, -1);
        Sprite[] sprites = Resources.LoadAll<Sprite>("Trey 1");

        foreach (Sprite s in sprites)
        {
            if (s.name == "Trey 1_0")
            {
                GetComponent<Image>().sprite = s;
                break;
            }
        }
    }

    protected override void Move()
    {
        base.Move();

        CheckForPromotion();
    }

    private bool MatchesState(int targetX, int targetY, CellState targetState)
    {
        CellState cellState = CellState.None;
        cellState = mCurrentCell.mBoard.ValidateCell(targetX, targetY, this);

        if (cellState == targetState)
        {
            mHighlightedCells.Add(mCurrentCell.mBoard.mAllCells[targetX, targetY]);
            return true;
        }

        return false;
    }

    private void CheckForPromotion()
    {
        // Target position
        int currentX = mCurrentCell.mBoardPosition.x;
        int currentY = mCurrentCell.mBoardPosition.y;
        // Promote when entering the last two ranks of the opponent side (rows 6-7 for white, 1-0 for black)
        bool inPromotionZone = mColor == Color.white
            ? currentY >= 5
            : currentY <= 2;

        if (inPromotionZone)
        {
            Color spriteColor = GetComponent<Image>().color;
            mPieceManager.PromotePiece(this, mCurrentCell, mColor, spriteColor);
        }
    }

    protected override void CheckPathing()
    {
        // Target position
        int currentX = mCurrentCell.mBoardPosition.x;
        int currentY = mCurrentCell.mBoardPosition.y;

        // Top left
        MatchesState(currentX - mMovement.z, currentY + mMovement.z, CellState.Enemy);

        // Forward
        MatchesState(currentX, currentY + mMovement.y, CellState.Free);

        // Top right
        MatchesState(currentX + mMovement.z, currentY + mMovement.z, CellState.Enemy);
    }
}
