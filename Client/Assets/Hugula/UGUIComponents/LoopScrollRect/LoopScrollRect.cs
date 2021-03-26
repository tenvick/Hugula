﻿// #define TEST
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hugula.UIComponents
{
    [XLua.LuaCallCSharp]
    public class LoopScrollRect : LoopScrollBase
    {
        protected override void ScrollLoopItem()
        {
            bool tween = isTweening;
            if (columns == 0 && (IsHorizontalScroll || tween))
            {
                m_HeadDataIndex = Mathf.FloorToInt(-m_ViewPointRect.x / (itemSize.x + this.halfPadding)); //头
                if (m_HeadDataIndex < 0) m_HeadDataIndex = 0;
                m_FootDataIndex = m_HeadDataIndex + pageSize > dataLength ? dataLength : m_HeadDataIndex + pageSize;
                for (int i = m_HeadDataIndex; i < m_FootDataIndex; i++)
                {
                    var item = GetLoopItemAt(i);
                    if (item.index != i)
                    {
                        RenderItem(item, i);
                    }
                }
            }
            else if (columns > 0 && (IsVerticalScroll || tween))
            {
                int cloumnIndex = Mathf.FloorToInt(-m_ViewPointRect.y / (itemSize.y + this.halfPadding));
                m_HeadDataIndex = Mathf.CeilToInt((float)(cloumnIndex * this.columns) / (float)this.columns) * columns; //
                if (m_HeadDataIndex < 0) m_HeadDataIndex = 0;
                m_FootDataIndex = m_HeadDataIndex + pageSize > dataLength ? dataLength : m_HeadDataIndex + pageSize;
                if (m_FootDataIndex > dataLength) m_FootDataIndex = dataLength;
                for (int i = m_HeadDataIndex; i < m_FootDataIndex; i++)
                {
                    var item = GetLoopItemAt(i);
                    if (item.index != i)
                    {
                        RenderItem(item, i);
                    }
                }

                // if (velocity.y > 0 || tweenDir.y > 0) //向上拖动
                // {
                //     int cloumnIndex = Mathf.FloorToInt (-m_ViewPointRect.y / (itemSize.y + this.halfPadding));
                //     m_HeadDataIndex = Mathf.CeilToInt ((float) (cloumnIndex * this.columns) / (float) this.columns) * columns; //
                //     if (m_HeadDataIndex < 0) m_HeadDataIndex = 0;
                //     m_FootDataIndex = m_HeadDataIndex + pageSize > dataLength ? dataLength : m_HeadDataIndex + pageSize;
                //     for (int i = m_HeadDataIndex; i < m_FootDataIndex; i++) {
                //         var item = GetLoopItemAt (i);
                //         if (item.index != i) {
                //             RenderItem (item, i);
                //         }
                //     }
                // } else if (velocity.y < 0 || tweenDir.y < 0) //向下拖动
                // {
                //     int cloumnIndex = Mathf.CeilToInt (-m_ViewPointRect.yMax / (itemSize.y + this.halfPadding));
                //     m_FootDataIndex = Mathf.CeilToInt ((float) (cloumnIndex * this.columns) / (float) this.columns) * columns; //
                //     if (m_FootDataIndex > dataLength) m_FootDataIndex = dataLength;
                //     m_HeadDataIndex = m_FootDataIndex - pageSize <= 0 ? 0 : m_FootDataIndex - pageSize;

                //     for (int i = m_HeadDataIndex; i < m_FootDataIndex; i++) {
                //         var item = GetLoopItemAt (i);
                //         if (item.index != i) {
                //             RenderItem (item, i);
                //         }
                //     }
                // }

            }
        }

        protected override void LayOut(LoopItem loopItem) //
        {

            RectTransform rectTran = loopItem.transform;
            int index = loopItem.index;
            var rect = rectTran.rect;
            rect.height = -Mathf.Abs(rect.height);
            Vector2 pos = Vector2.zero;
            if (this.columns == 0) //单行
            {
                pos.x = (rect.width + this.halfPadding) * index + this.halfPadding; // + rect.width * .5f;
                pos.y = -halfPadding;
            }
            else if (columns == 1) //单列 需要宽度适配
            {
                pos.x = halfPadding;
                pos.y = (rect.height - this.halfPadding) * index - this.halfPadding; // rect.height * .5f ;
            }
            else // 多行
            {
                int x = index % columns;
                pos.x = (rect.width + this.halfPadding) * x + this.halfPadding; // + rect.width * .5f;
                int y = index / columns;
                pos.y = (rect.height - this.halfPadding) * y - this.halfPadding; // rect.height * .5f ;
            }
            rect.position = pos;
            pos = pos + m_ContentLocalStart; //开始位置

            loopItem.rect = rect;

            // Debug.LogFormat("SetInsetAndSizeFromParentEdge pos={0},width={1} ", pos, rectTran.rect.width);
            rectTran.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, pos.x, rectTran.rect.width);
            rectTran.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, -pos.y, rectTran.rect.height);

        }

        protected override void CalcBounds()
        {

            if (content != null)
            {
                var rect = content.rect;
                Vector2 delt = new Vector2(rect.width, rect.height);
                if (columns <= 0) //只有一行，为了高度适配不设置sieDelta.y
                {
                    delt.x = dataLength * (itemSize.x + this.halfPadding) + this.halfPadding;
                }
                else if (columns == 1) //只有一列的时候为了 宽度适配不设置sieDelta.x
                {
                    delt.y = (itemSize.y + this.halfPadding) * dataLength + this.halfPadding;
                }
                else
                {
                    delt.x = columns * (itemSize.x + this.halfPadding) + this.halfPadding;
                    int y = (int)Mathf.Ceil((float)dataLength / (float)columns);
                    delt.y = (itemSize.y + this.halfPadding) * y + this.halfPadding;
                }

                content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, delt.x);
                content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, delt.y);
            }
        }

        ///<summary>
        /// 通过属性的方式指定滚动到制定索引 
        /// 为了数据绑定
        ///</summary>
        public int setScrollIndex
        {
            get { return this.selectedIndex; }
            set
            {
                ScrollToIndex(value);
            }
        }

        ///<summary>
        /// 滚动到指定索引
        ///</summary>
        public void ScrollToIndex(int idx)
        {

            if (isTweening) return;
            if (idx < 0) idx = 0;
            if (idx >= dataLength) idx = dataLength - 1;
            if (idx < 0) return;

            var cpos = Vector2.zero; //开始位置
            Vector2 curr = content.anchoredPosition;

            int columns = base.columns;
            if (columns == 0) //x轴 
            {
                cpos.x = -(itemSize.x + this.halfPadding) * idx - m_ContentLocalStart.x; //开始位置
            }
            else
            {
                int row = idx / columns;
                cpos.y = (itemSize.y + this.halfPadding) * row + m_ContentLocalStart.y; //开始位置
            }

            m_Coroutine = StartCoroutine(TweenMoveToPos(curr, cpos, 0.5f));

        }

        Vector2 tweenDir;
        //在使用scrollto的时候需要控制drag
        protected bool isTweening
        {
            get
            {
                return !tweenDir.Equals(Vector2.zero);
            }
        }

        protected bool IsVerticalScroll
        {
            get
            {
                return velocity.y != 0 || scrollBarDragging;
            }

        }

        protected bool IsHorizontalScroll
        {
            get
            {
                return velocity.x != 0 || scrollBarDragging;
            }
        }

        protected IEnumerator TweenMoveToPos(Vector2 pos, Vector2 v2Pos, float delay)
        {
            var waitForend = new WaitForEndOfFrame();
            float passedTime = 0f;
            tweenDir = (v2Pos - pos).normalized;

            while (!tweenDir.Equals(Vector2.zero))
            {
                yield return waitForend;
                passedTime += Time.deltaTime;
                Vector2 vCur;
                if (passedTime >= delay)
                {
                    vCur = v2Pos;
                    tweenDir = Vector2.zero;
                    StopCoroutine(m_Coroutine);
                    m_Coroutine = null;
                }
                else
                {
                    vCur = Vector2.Lerp(pos, v2Pos, passedTime / delay);
                }
                content.anchoredPosition = vCur;
            }

        }
        protected Coroutine m_Coroutine = null;

    }

    public static class RectTransformExtension
    {
        /// <summary>
        /// 判断宽度适配还是高度适配 0 宽度x水平  1 高度y竖直
        /// </summary>
        /// <param name="axis">The axis to set: 0 for horizontal, 1 for vertical.</param>
        public static bool IsMatchOffset(this RectTransform transform, RectTransform.Axis axis)
        {
            var a = (int)axis;
            return transform.offsetMin[a] != transform.offsetMax[a];
        }
    }
}