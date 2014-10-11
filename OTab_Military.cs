using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using System.Linq;
using VerseBase;


namespace MilitaryOverview
{
    public class OTab_Military : OverviewTab
    {
        private const float TopAreaHeight = 40f;
        private const float LabelRowHeight = 50f;
        private const float PawnRowHeight = 30f;
        private const float NameColumnWidth = 175f;
        private const float NameLeftMargin = 15f;
        private const float StrikethroughY = 15f;
        private Vector2 scrollPosition = Vector2.zero;
        private float workColumnSpacing = -1f;
        private static readonly Color DisabledSkillColor = new Color(1f, 1f, 1f, 0.5f);
        private static Texture2D SkillBarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));

        public OTab_Military() {
            this.title = "Military";
            this.order = 999;
        }

        public override void PanelOnGUI( Rect fillRect ) {
            Rect innerRect = fillRect.GetInnerRect( 10f );
            GUI.BeginGroup( innerRect );
            Rect position = new Rect( 0f, 0f, innerRect.width, 40f );
            GUI.BeginGroup( position );
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Text.Alignment = TextAnchor.UpperLeft;
            Rect rect = new Rect( 5f, 5f, 140f, 30f );
            float num = position.width / 3f;
            float num2 = position.width * 2f / 3f;
            GUI.color = new Color( 1f, 1f, 1f, 0.5f );
            Text.Alignment = TextAnchor.UpperCenter;
            Text.Alignment = TextAnchor.UpperLeft;
            GUI.EndGroup();
            Rect position2 = new Rect( 0f, 40f, innerRect.width, innerRect.height - 40f );
            GUI.BeginGroup( position2 );
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            float height = (float)Find.ListerPawns.FreeColonistsCount * 30f;
            Rect outRect = new Rect( 0f, 50f, position2.width, position2.height - 50f );
            Rect rect4 = new Rect( 0f, 0f, position2.width - 16f, height );
            this.workColumnSpacing = (rect4.width - 175f) / (float)DefDatabase<WorkTypeDef>.AllDefs.Count<WorkTypeDef>();
            float num3 = 175f;
            int num4 = 0;
            foreach ( WorkTypeDef current in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder ) {
                float num5 = num3 + 15f;
                Rect rect5 = new Rect( num5 - 100f, 0f, 200f, 30f );
                if ( num4 % 2 == 1 ) {
                    rect5.y += 20f;
                }
                Text.Alignment = TextAnchor.MiddleCenter;

                Widgets.Label( rect5, current.gerundLabel );               

                GUI.color = new Color( 1f, 1f, 1f, 0.3f );
                Widgets.DrawLineVertical( new Vector2( num5, rect5.yMax - 7f ), 50f - rect5.yMax + 7f );
                Widgets.DrawLineVertical( new Vector2( num5 + 1f, rect5.yMax - 7f ), 50f - rect5.yMax + 7f );
                GUI.color = Color.white;
                num3 += this.workColumnSpacing;
                num4++;
            }
            this.scrollPosition = Widgets.BeginScrollView( outRect, this.scrollPosition, rect4 );
            float num6 = 0f;
            foreach ( Pawn current2 in Find.ListerPawns.FreeColonists ) {
                GUI.color = new Color( 1f, 1f, 1f, 0.2f );
                Widgets.DrawLineHorizontal( new Vector2( 0f, num6 ), rect4.width );
                GUI.color = Color.white;
                this.DrawPawnRow( current2, num6, rect4 );
                num6 += 30f;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.EndGroup();
        }

        private void DrawPawnRow(Pawn p, float rowY, Rect fillRect)
		{
			Rect position = new Rect(0f, rowY, fillRect.width, 30f);
			if (position.Contains(Event.current.mousePosition))
			{
				GUI.DrawTexture(position, TexUI.HighlightTex);
			}
			Rect rect = new Rect(0f, rowY, 175f, 30f);
			Rect innerRect = rect.GetInnerRect(3f);
			if (p.healthTracker.Health < p.healthTracker.MaxHealth - 1)
			{
				Rect screenRect = new Rect(rect);
				screenRect.xMin -= 4f;
				screenRect.yMin += 4f;
				screenRect.yMax -= 6f;
				Widgets.FillableBar(screenRect, (float)p.healthTracker.Health / (float)p.healthTracker.MaxHealth, PawnUIOverlay.HealthTex, false, BaseContent.ClearTex);
			}
			if (rect.Contains(Event.current.mousePosition))
			{
                GUI.DrawTexture(innerRect, TexUI.HighlightTex);
			}
			Text.Font = GameFont.Small;
			Text.Alignment = TextAnchor.MiddleLeft;
			Rect rect2 = new Rect(rect);
			rect2.xMin += 15f;
			Widgets.Label(rect2, p.Label);
			if (Widgets.InvisibleButton(rect))
			{
				Find.LayerStack.FirstLayerOfType<Dialog_Overview>().Close();
				Find.CameraMap.JumpTo(p.Position);
				return;
			}
			TipSignal tooltip = p.GetTooltip();
			tooltip.text = "ClickToJumpTo".Translate() + "\n\n" + tooltip.text;
			TooltipHandler.TipRegion(rect, tooltip);
			float num = 175f;
			Text.Font = GameFont.Medium;
            
            Vector2 topLeft = new Vector2( num, rowY + 2.5f );

            SkillRecord skill = p.skills.GetSkill( SkillDefOf.Shooting );
            if ( !skill.TotallyDisabled ) {
                Widgets.FillableBar( new Rect( topLeft.x, topLeft.y, 240f, 24f ), (float)skill.level / 20f, SkillBarFillTex, false, null );
            }

            if ( skill.TotallyDisabled ) {
                GUI.color = DisabledSkillColor;
                Widgets.Label( new Rect( topLeft.x, topLeft.y, 240f, 24f ), "-");
                GUI.color = Color.white;
            }

            //foreach (WorkTypeDef current in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder)
            //{
                

            //    Vector2 topLeft = new Vector2(num, rowY + 2.5f);
            //    //WidgetsWork.DrawWorkBoxFor(topLeft, p, current);
            //    Widgets.Label( new Rect( topLeft.x, topLeft.y, 25f, 25f ), p.skills.GetSkill( SkillDefOf.Shooting ).level.ToString() );

            //    Rect rect3 = new Rect(topLeft.x, topLeft.y, 25f, 25f);
            //    TooltipHandler.TipRegion(rect3, WidgetsWork.TipForPawnWorker(p, current));
            //    num += this.workColumnSpacing;
            //}
			if (p.Downed)
			{
				GUI.color = Color.red;
				Widgets.DrawLineHorizontal(new Vector2(175f, rowY + 15f), num - 175f - 17f);
				GUI.color = Color.white;
			}
		}

        //private void DrawSkill( SkillRecord skill, Vector2 topLeft, SkillUI.SkillDrawMode drawMode ) {
        //    Rect rect = new Rect( topLeft.x, topLeft.y, 240f, 24f );
        //    if ( rect.Contains( Event.current.mousePosition ) ) {
        //        GUI.DrawTexture( rect, GenUI.HighlightTex );
        //    }
        //    GUI.BeginGroup( rect );
        //    Text.Alignment = TextAnchor.MiddleLeft;
        //    Rect rect2 = new Rect( 6f, 0f, -1f + 6f, rect.height );
        //    rect2.yMin += 3f;
        //    Widgets.Label( rect2, skill.def.skillLabel );
        //    Rect position = new Rect( rect2.xMax, 0f, 24f, 24f );

        //    if ( !skill.TotallyDisabled ) {
        //        Rect screenRect = new Rect( position.xMax, 0f, rect.width - position.xMax, rect.height );
        //        Widgets.FillableBar( screenRect, (float)skill.level / 20f, SkillUI.SkillBarFillTex, false, null );
        //    }
        //    Rect rect3 = new Rect( position.xMax + 4f, 0f, 999f, rect.height );
        //    rect3.yMin += 3f;

        //    Text.Alignment = TextAnchor.MiddleLeft;

        //    GUI.color = Color.white;
        //    GUI.EndGroup();
        //}
    }
}
