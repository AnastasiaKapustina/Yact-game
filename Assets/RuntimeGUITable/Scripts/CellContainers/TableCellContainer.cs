using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using UnityEngine.EventSystems;

namespace UnityUITable
{

	[ExecuteInEditMode]
	public class TableCellContainer : CellContainer, IPointerClickHandler
	{

		public Image background;
		public Image outline;
		protected int rowIndex;

		public void Initialize(Table table, int rowIndex)
		{
			this.rowIndex = rowIndex;
			gameObject.name += rowIndex;
			base.Initialize(table);
		}

		protected override void Update()
		{
			if (table == null)
				return;
			base.Update();
			background.color = table.GetRowBGColor(rowIndex);
			outline.color = table.lineColor;
			outline.sprite = table.outlineSprite;
		}

		public override void SetRowIndex(int rowIndex)
		{
			base.SetRowIndex(rowIndex);
			this.rowIndex = rowIndex;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
            table.SetSelected(rowIndex);
            Game.Gameplay.updateScore(rowIndex);
		}

	}

}