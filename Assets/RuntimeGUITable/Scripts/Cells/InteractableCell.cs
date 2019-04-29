using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityUITable
{

	public interface InteractableCellInterface
	{
		bool interactable { get; set; }
	}

	public abstract class InteractableCell<StyleType> :
		StyleableTableCell<StyleType>,
		InteractableCellInterface
		where StyleType : InteractableCellStyle
	{

		public bool interactable { get; set; }

		protected void SetValue(object o, object value)
		{
			if (!interactable)
				return;
			property.SetValue(o, value);
		}

	}

}
