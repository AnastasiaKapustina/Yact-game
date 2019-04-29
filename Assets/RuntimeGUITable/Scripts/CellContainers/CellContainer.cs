using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

namespace UnityUITable
{

	[ExecuteInEditMode]
	public class CellContainer : MonoBehaviour
	{

		LayoutElement layoutElement;
		public Transform content;
		public Table table;
		public TableCell cellInstance { get; private set; }

		public void Initialize(Table table)
		{
			this.table = table;
			layoutElement = gameObject.AddComponent<LayoutElement>();
			Update();
		}

		public TableCell CreateCellContent(TableCell cellPrefab)
		{
			cellInstance = GameObjectUtils.InstantiatePrefab(cellPrefab, content);
			return cellInstance;
		}

		public virtual void SetRowIndex(int rowIndex)
		{
			transform.SetSiblingIndex(rowIndex + 1);
		}

		protected virtual void Update()
		{
			if (table == null)
				return;
			layoutElement.preferredHeight = table.rowHeight;
		}

		public virtual void UpdateContent()
		{
			cellInstance.UpdateContent();
		}

		public void UpdateStyle()
		{
			cellInstance.UpdateStyle();
		}

	}

}