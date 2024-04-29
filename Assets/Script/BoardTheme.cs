using UnityEngine;
namespace Chess.Game {
	[CreateAssetMenu (menuName = "Theme/Board")]
	public class BoardTheme : ScriptableObject {

		public SquareColours lightSquares;
		public SquareColours darkSquares;

        public SquareSprites lightSquaresSprite;
        public SquareSprites darkSquaresSprite;

        [System.Serializable]
		public struct SquareColours {
			public Color normal;
			public Color legal;
			public Color selected;
			public Color moveFromHighlight;
			public Color moveToHighlight;
		}

        [System.Serializable]
        public struct SquareSprites
        {
            public Sprite normal;
            public Sprite legal;
            public Sprite selected;
            public Sprite moveFromHighlight;
            public Sprite moveToHighlight;
            public Sprite deathHighlight;
            public Sprite kingDeathHighlight;
        }
    }
}