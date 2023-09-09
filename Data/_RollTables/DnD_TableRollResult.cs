using QuizCanners.Inspect;
using QuizCanners.Migration;
using QuizCanners.Utils;
using System;
using UnityEngine;

namespace Dungeons_and_Dragons.Tables
{
    [Serializable]
    public class TableRollResult : IPEGI, //IGotName, 
        IPEGI_ListInspect, ISerializationCallbackReceiver, ICfg
    {
      //  [SerializeField] private string _name;
        [SerializeField] private string tableKey;

        public RolledTable.Result result = new();

        private RandomElementsRollTablesDictionary Tables => Singleton.GetValue<Singleton_DnD, RandomElementsRollTablesDictionary>(s => s.DnDPrototypes.RollTables);

        public void Roll()
        {
            using (TryGetTableDisposable(out var table, out var tableFound))
            {
                if (tableFound)
                    table.Roll(result);
                else
                    Debug.LogError("Table not found");
            }
        }


        private IDisposable TryGetTableDisposable(out RandomElementsRollTables table, out bool tableFound) 
        {
            tableFound = false;

            if (tableKey.IsNullOrEmpty() || Tables == null) 
            {
                table = null;
                return null;
            }

            tableFound = Tables.TryGetValue(tableKey, out table) && table;

            if (tableFound) 
            {
                return result.AddAndUse(table);
            }
            return null;
        }

        #region Encode & Decode

        public CfgEncoder Encode() => new CfgEncoder()
           // .Add_String("n", _name)
            .Add_String("t", tableKey)
            .Add("r", result);

        public void DecodeTag(string key, CfgData data)
        {
            switch (key) 
            {
                //case "n": _name = data.ToString(); break;
                case "t": tableKey = data.ToString(); break;
                case "r": result.Decode(data); break;
            }
        }

        [SerializeField] private CfgData _data;
        public void OnBeforeSerialize() =>  _data = result.Encode().CfgData;
        public void OnAfterDeserialize()
        {
            result = new RolledTable.Result();
            _data.DecodeOverride(ref result);
        }

        #endregion

        #region Inspector
        // public string NameForInspector { get => _name; set => _name = value; }
        public override string ToString()
        {
            using (TryGetTableDisposable(out var table, out var tableFound))
            {
                if (tableFound)
                {
                    return table.GetRolledElementName(result, shortText: true);
                }
            }

            return tableKey;
        }
        public void Inspect()
        {
            //pegi.Edit(ref _name);

            "Table".PegiLabel(70).Select(ref tableKey, Tables);
            Icon.Save.Click(OnBeforeSerialize);
            Icon.Load.Click(OnAfterDeserialize);

            using (TryGetTableDisposable(out var table, out var tableFound))
            {
                if (tableFound)
                {
                    Icon.Dice.Click(Roll);
                    pegi.ClickHighlight(table).Nl();
                    table.Inspect(result);
                }
            }
        }

        public void InspectInList(ref int edited, int index)
        {
            pegi.Select(ref tableKey, Tables);

            if (Icon.Enter.Click())
                edited = index;

            using (TryGetTableDisposable(out var table, out var tableFound))
            {
                if (tableFound)
                {
                    pegi.ClickHighlight(table);

                    Icon.Dice.Click(() => Roll());

                    using (pegi.Indent())
                    {
                        pegi.Nl();
                        table.GetRolledElementName(result, shortText: true).PegiLabel(pegi.Styles.HintText).Write();
                    }
                }
            }
        }

        #endregion
    }
}