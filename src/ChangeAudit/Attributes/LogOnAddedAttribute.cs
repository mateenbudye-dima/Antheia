using System;
using System.Collections.Generic;
using System.Text;

namespace Dima.ChangeAudit.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class LogOnAddedAttribute : Attribute { }