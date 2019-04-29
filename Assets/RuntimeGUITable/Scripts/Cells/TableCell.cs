using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;

namespace UnityUITable
{

	[ExecuteInEditMode]
	public abstract class TableCell : MonoBehaviour
	{

		protected MemberInfo member;
		public int elmtIndex { get; private set; }
		public object obj { get { return table.GetSortedElements()[elmtIndex]; } }

		TableColumnInfo columnInfo;
		protected TableCellStyle cellStyle { get { return columnInfo.CellStyle; } }
		protected Table table;

		PropertyOrFieldInfo _property;
		protected PropertyOrFieldInfo property
		{
			get
			{
				if (_property == null) _property = new PropertyOrFieldInfo(member);
				return _property;
			}
		}

		public virtual Type StyleType { get { return null; } }

		protected static bool IsOfCompatibleType(MemberInfo member, params Type[] compatibleTypes)
		{
			return compatibleTypes != null && compatibleTypes.Any(t => t.IsAssignableFrom(PropertyOrFieldInfo.GetPropertyOrFieldType(member)));
		}

		public abstract bool IsCompatibleWithMember(MemberInfo member);

		/// <summary>
		/// Override to define the priority of the cell type, depending on the member associated.
		/// </summary>
		/// <returns>The priority</returns>
		/// <param name="member">The member associated with this column.</param>
		public virtual int GetPriority(MemberInfo member)
		{
			return 0;
		}

		public void Initialize(Table table, int elmtIndex, TableColumnInfo columnInfo)
		{
			this.table = table;
			this.elmtIndex = elmtIndex;
			this.columnInfo = columnInfo;
			this.member = columnInfo.GetMember();
			UpdateContent();
			UpdateStyle();
		}



		public abstract void UpdateContent();

		public virtual void UpdateStyle() { }

	}

}
