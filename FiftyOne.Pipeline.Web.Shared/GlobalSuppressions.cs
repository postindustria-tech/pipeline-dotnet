// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", 
    "CA1716:Identifiers should not match keywords", 
    Justification = "This would be a breaking change that we are" +
        "not going to resolve at the current time.", 
    Scope = "namespaceanddescendants", 
    Target = "FiftyOne.Pipeline.Web.Shared")]
