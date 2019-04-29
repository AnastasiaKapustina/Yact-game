using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityUITable
{

	public class ButtonCell : StyleableTableCell<ButtonCellStyle>
	{

		public Button button;
		Action OnButtonClickAction;

		public void InitializeSpecialAction(Table table, int elmtIndex, Action OnButtonClickAction, TableColumnInfo info)
		{
			base.Initialize(table, elmtIndex, info);
			this.OnButtonClickAction = OnButtonClickAction;
		}

		public override int GetPriority(MemberInfo member)
		{
			return 10;
		}

		public override bool IsCompatibleWithMember(MemberInfo member)
		{
			return (member.MemberType == MemberTypes.Method && ((MethodInfo)member).GetParameters().Length == 0);
		}

		public override void UpdateContent()
		{
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(OnButtonClicked);
		}

		void OnButtonClicked()
		{
			if (OnButtonClickAction == null)
				((MethodInfo)member).Invoke(obj, new object[] { });
			else
				OnButtonClickAction.Invoke();
		}

	}

}
