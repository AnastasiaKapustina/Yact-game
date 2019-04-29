using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

namespace UnityUITable
{

	[ExecuteInEditMode]
	public class DeleteColumn : TableColumn
	{

		protected override CellContainer CreateCell(int rowIndex)
		{
			CellContainer cellContainer = GameObjectUtils.InstantiatePrefab(table.emptyCellContainerPrefab, transform);
			cellContainer.Initialize(table);
			cellContainers.Add(cellContainer);
			ButtonCell cell = (ButtonCell)cellContainer.CreateCellContent(info.CellPrefab);
			cell.InitializeSpecialAction(
				table,
				rowIndex,
				() =>
				{
					object obj = new PropertyOrFieldInfo(table.targetCollection.GetMember()).GetValue(table.targetCollection.GetComponent());
					MethodInfo deleteMethod = obj.GetType().GetMethod("RemoveAt");
					if (deleteMethod != null)
						deleteMethod.Invoke(obj, new object[] { rowIndex });
					else
						Debug.LogError("There is no RemoveAt method on this collection.");
				},
				info);
			return cellContainer;
		}

		protected override void CreateTitleCell()
		{
			AddEmptyCell("Empty Title");
		}

	}

}
