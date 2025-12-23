using UnityEngine;
using UnityEngine.UI;

public class Queen : BasePiece
{
    public override void Setup(Color newTeamColor, Color32 newSpriteColor, PieceManager newPieceManager)
    {
        // Base setup
        base.Setup(newTeamColor, newSpriteColor, newPieceManager);

        // Queen movement ranges (horizontal, vertical, diagonal)
        // Use positive magnitudes; direction is handled in BasePiece
        mMovement = new Vector3Int(0, 0, 1);
        Sprite[] sprites = Resources.LoadAll<Sprite>("W");

        foreach (Sprite s in sprites)
        {
            if (s.name == "White_Queen")
            {
                GetComponent<Image>().sprite = s;
                break;
            }
        }
    }

    private bool MatchesState(int targetX, int targetY, CellState targetState)
    {
        CellState cellState = mCurrentCell.mBoard.ValidateCell(targetX, targetY, this);

        if (cellState == targetState)
        {
            mHighlightedCells.Add(mCurrentCell.mBoard.mAllCells[targetX, targetY]);
            return true;
        }

        return false;
    }

    protected override void CheckPathing()
    {
        // Keep default queen pathing (diagonals, ranks, files, etc.)
        base.CheckPathing();

        // Add custom rule: on the first move, queen may move forward two squares like a pawn.
        // Determine current position
        int currentX = mCurrentCell.mBoardPosition.x;
        int currentY = mCurrentCell.mBoardPosition.y;

        // Only allow a special forward two-square move on the very first move,
        // regardless of whether the first square is blocked. Direction depends on color.
        // Also block ALL forward vertical squares on the first move except the 2-square.
        if (mIsFirstMove)
        {
            int forward = (mColor == Color.white) ? 1 : -1;

            // Remove any forward vertical moves added by base pathing
            mHighlightedCells.RemoveAll(cell =>
                cell.mBoardPosition.x == currentX &&
                ((forward == 1 && cell.mBoardPosition.y > currentY) ||
                 (forward == -1 && cell.mBoardPosition.y < currentY))
            );

            // Re-add only the 2-square forward if it's free
            MatchesState(currentX, currentY + (forward * 2), CellState.Free);
        }
    }
}
