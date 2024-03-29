using System;
using System.Collections.Generic;
using QuizCanners.Inspect;
using QuizCanners.Utils;
using UnityEngine;


namespace Dungeons_and_Dragons.Tables
{
    [CreateAssetMenu(fileName = FILE_NAME,  menuName = SO_CREATE_PATH + FILE_NAME)]
    public class MonsterRollTable : RollTableGeneric<MonsterRollTable.Element> {

        public const string FILE_NAME = "Random Monster";
        
        public List<Element> elements;

        [NonSerialized] private readonly RandomRollCache _randomRollsCache = new();

        public Element this[RolledTable.Result roll] => Get(elements, roll.Roll);

        public Element this[RanDndSeed seed] => Get(elements, _randomRollsCache.Get(seed, ()=> RollDices(seed)));

        protected override List<Element> List { get => elements; set => elements = value; }

        public override string GetRolledElementName(RanDndSeed seed, IConceptValueProvider provider, bool shortText)
        {
            var el = this[seed];
            if (el != null)
                return el.NameForInspector;
            return "Null Monster";
        }

        protected override void RollInternal(RolledTable.Result result)
        {
            result.Roll = RollDices();
        }

        public override string GetRolledElementName(RolledTable.Result result, bool shortText)
        {
            if (!result.IsRolled)
                return "Not Rolled";

            var el = Get(elements, result.Roll);
            if (el != null)
                return el.Name;

            return "No Element for {0}".F(result.Roll);
        }

        #region Inspector
        protected override bool EditList() => "{0} {1}".F(_dicesToRoll.ToRollTableDescription(), name).PegiLabel().Edit_List(elements).Nl();

        public override void Inspect()
        {
            base.Inspect();

            if ("Get Random".PegiLabel().Click())
                pegi.GameView.ShowNotification(GetRandom().Name);
        }

        protected override void InspectInList_Internal(ref int edited, int index, RolledTable.Result result)
        {
            if (Icon.Enter.Click())
                edited = index;
        }

        internal override void SelectInternal(RolledTable.Result result)
        {
            Element el = this[result];
            if ("Enemy".PegiLabel(70).Select(ref el, elements) && el != null)
                result.Roll = GetTargetRoll(elements, el);

            pegi.ClickHighlight(this);
        }

        public override void Inspect(RolledTable.Result result)
        {
            if (Icon.Dice.Click())
                Roll(result);

            SelectInternal(result);

            pegi.Nl();
        }

        #endregion


        [Serializable]
        public class Element : RollTableElementBase, IGotName
        {
            public string Name;

            public string NameForInspector { get => Name; set => Name = value; }

            public override string GetRolledElementName(RanDndSeed seed, IConceptValueProvider provider, bool shortText) => Name;

            public override void InspectInList(ref int edited, int ind)
            {
                base.InspectInList(ref edited, ind);
                
                if (!Data)
                    "No Manager".PegiLabel().WriteWarning();
                else if (!Data)
                    "No Prototypes in Manager".PegiLabel().WriteWarning();
                else
                    pegi.Select(ref Name, Data.Monsters);
            }
        }
    }
    
    [PEGI_Inspector_Override(typeof(MonsterRollTable))] internal class MonsterRollTableDrawer : PEGI_Inspector_Override { }
}