﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CruciatusElement.cs" company="2GIS">
//   Cruciatus
// </copyright>
// <summary>
//   Представляет базу для элементов управления.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Cruciatus.Elements
{
    #region using

    using System;
    using System.Collections.Generic;
    using System.Windows.Automation;

    using Cruciatus.Core;
    using Cruciatus.Exceptions;

    using NLog;

    #endregion

    /// <summary>
    /// Базовый класс для элементов.
    /// </summary>
    public class CruciatusElement
    {
        protected static readonly Logger Logger = CruciatusFactory.Logger;

        private AutomationElement _instance;

        internal CruciatusElement(CruciatusElement parent, AutomationElement element, By strategy)
        {
            Parent = parent;
            Instanse = element;
            GetStrategy = strategy;
        }

        public CruciatusElement(CruciatusElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            Instanse = element.Instanse;
            Parent = element;
            GetStrategy = element.GetStrategy;
        }

        public CruciatusElement(CruciatusElement parent, By getStrategy)
        {
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }

            Parent = parent;
            GetStrategy = getStrategy;
        }

        internal AutomationElement Instanse
        {
            get
            {
                if (_instance == null)
                {
                    _instance = CruciatusCommand.FindFirst(Parent.Instanse, GetStrategy);
                }

                if (_instance == null)
                {
                    throw new CruciatusException("ELEMENT NOT FOUND");
                }

                return _instance;
            }

            set
            {
                _instance = value;
            }
        }

        internal CruciatusElement Parent { get; set; }

        public By GetStrategy { get; internal set; }

        public CruciatusElementProperties Properties
        {
            get
            {
                return new CruciatusElementProperties(Instanse);
            }
        }

        public virtual CruciatusElement GetByUid(string value)
        {
            return Get(By.Uid(value));
        }

        public virtual CruciatusElement GetByName(string value)
        {
            return Get(By.Name(value));
        }

        public virtual CruciatusElement GetByPath(string value)
        {
            return Get(By.Path(value));
        }

        public virtual CruciatusElement Get(By strategy)
        {
            return CruciatusCommand.FindFirst(this, strategy);
        }

        public IEnumerable<CruciatusElement> GetAll(By strategy)
        {
            return CruciatusCommand.FindAll(this, strategy);
        }

        public void Click()
        {
            Click(CruciatusFactory.Settings.ClickButton);
        }

        public void Click(MouseButton button)
        {
            Click(button, ClickStrategies.None, false);
        }

        public void Click(MouseButton button, ClickStrategies strategy)
        {
            Click(button, strategy, false);
        }

        public void Click(MouseButton button, ClickStrategies strategy, bool doubleClick)
        {
            if (!Instanse.Current.IsEnabled)
            {
                Logger.Error("Element '{0}' not enabled. Click failed.", ToString());
                throw new ElementNotEnabledException("NOT CLICK");
            }

            if (strategy == ClickStrategies.None)
            {
                strategy = ~strategy;
            }

            if (strategy.HasFlag(ClickStrategies.ClickablePoint))
            {
                if (CruciatusCommand.TryClickOnClickablePoint(button, this, doubleClick))
                {
                    return;
                }
            }

            if (strategy.HasFlag(ClickStrategies.BoundingRectangleCenter))
            {
                if (CruciatusCommand.TryClickOnBoundingRectangleCenter(button, this, doubleClick))
                {
                    return;
                }
            }

            if (strategy.HasFlag(ClickStrategies.InvokePattern))
            {
                if (CruciatusCommand.TryClickUsingInvokePattern(this, doubleClick))
                {
                    return;
                }
            }

            Logger.Error("Click on '{0}' element failed", ToString());
            throw new CruciatusException("NOT CLICK");
        }

        public void DoubleClick()
        {
            DoubleClick(CruciatusFactory.Settings.ClickButton);
        }

        public void DoubleClick(MouseButton button)
        {
            DoubleClick(button, ClickStrategies.None);
        }

        public void DoubleClick(MouseButton button, ClickStrategies strategy)
        {
            Click(button, strategy, true);
        }

        public void SetText(string text)
        {
            if (!Instanse.Current.IsEnabled)
            {
                Logger.Error("Element '{0}' not enabled. Set text failed.", ToString());
                throw new ElementNotEnabledException("NOT SET TEXT");
            }

            Click(MouseButton.Left, ClickStrategies.ClickablePoint | ClickStrategies.BoundingRectangleCenter);

            CruciatusFactory.Keyboard.SendCtrlA().SendBackspace().SendText(text);
        }

        public string Text()
        {
            return Text(GetTextStrategies.None);
        }

        public string Text(GetTextStrategies strategy)
        {
            if (strategy == GetTextStrategies.None)
            {
                strategy = ~strategy;
            }

            string text;
            if (strategy.HasFlag(GetTextStrategies.TextPattern))
            {
                if (CruciatusCommand.TryGetTextUsingTextPattern(this, out text))
                {
                    return text;
                }
            }

            if (strategy.HasFlag(GetTextStrategies.ValuePattern))
            {
                if (CruciatusCommand.TryGetTextUsingValuePattern(this, out text))
                {
                    return text;
                }
            }

            Logger.Error("Get text from '{0}' element failed.", ToString());
            throw new CruciatusException("NO GET TEXT");
        }

        public override string ToString()
        {
            var typeName = Instanse.Current.ControlType.ProgrammaticName;
            var uid = Instanse.Current.AutomationId;
            var name = Instanse.Current.Name;
            var str = string.Format("{0}{1}{2}", 
                                    "type: " + typeName, 
                                    string.IsNullOrEmpty(uid) ? string.Empty : ", uid: " + uid, 
                                    string.IsNullOrEmpty(name) ? string.Empty : ", name: " + name);
            return str;
        }
    }
}
