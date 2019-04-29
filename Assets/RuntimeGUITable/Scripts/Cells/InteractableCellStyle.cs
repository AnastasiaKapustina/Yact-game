using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityUITable
{

	public abstract class InteractableCellStyle : TableCellStyle
	{

		static void ApplyInteractable(TableCell cell, bool v)
		{
			cell.GetComponentsInChildren<Graphic>().ForEach(g => g.raycastTarget = v);
			((InteractableCellInterface)cell).interactable = v;
		}

		public BoolSetting interactable = new BoolSetting(
			ApplyInteractable,
			(cell) => cell.GetComponentInChildren<Graphic>().raycastTarget);

	}

}
