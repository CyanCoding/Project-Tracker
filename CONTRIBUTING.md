# Contributing to the Project Tracker
Thanks for showing your interest in the Project Tracker program!
Before you get involved, take a look at the section that pertains
to your type of contribution:

**Table of contents**:
* [Submitting a bug report](https://github.com/CyanCoding/Project-Tracker/blob/master/CONTRIBUTING.md#submitting-a-bug-report)
* [Submitting a feature request](https://github.com/CyanCoding/Project-Tracker/blob/master/CONTRIBUTING.md#submitting-a-feature-request)
* [Backend contribution](https://github.com/CyanCoding/Project-Tracker/blob/master/CONTRIBUTING.md#backend-contribution)
* [Frontend contribution](https://github.com/CyanCoding/Project-Tracker/blob/master/CONTRIBUTING.md#frontend-contribution)
* [Fixing documentation](https://github.com/CyanCoding/Project-Tracker/blob/master/CONTRIBUTING.md#fixing-documentation)


**Before contributing**, make sure you're familiar with the [Code of Conduct](https://github.com/CyanCoding/Project-Tracker/blob/master/CODE_OF_CONDUCT.md).
Everyone participating in this project is expected to uphold the code of conduct.
Any user found to be violating the code of conduct will be punished
based on the severity and frequency of the violation(s).

If you spot someone breaking the code of conduct, have any questions
about contributing, or questions about the project itself, 
please contact me at CyanCoding@users.noreply.github.com.

## How can I contribute?
You can contribute by submitting a bug report, submitting a feature request, coding backend/frontend, or
by fixing documentation.

### Submitting a bug report
Noticed something off about the Project Tracker? Submit an
[issue](https://github.com/CyanCoding/Project-Tracker/issues)! Before
reporting a bug, make sure that you can recreate it. Instead of creating
a broad and hard-to-fix issue like "Project Tracker crashes", try and find
what's going wrong and what leads up to it, such as "Project Tracker crashes
directly after installation". All bug reports should be tagged with the 
`bug` tag.

If you are intending to fix the bug, go ahead and read 
[Backend contribution](https://github.com/CyanCoding/Project-Tracker/blob/master/CONTRIBUTING.md#backend-contribution)
**and** 
[Frontend contribution](https://github.com/CyanCoding/Project-Tracker/blob/master/CONTRIBUTING.md#frontend-contribution). 
Even if you're only intending to code backend
or frontend, it's important that you know what is required of any kind of
code contribution. When you create the issue, **make sure to assign yourself**. 
Any issue left unassigned will typically be assigned to the owner, [@CyanCoding](https://github.com/CyanCoding).

### Submitting a feature request
Have an idea about an improvement to the Project Tracker? Create an 
[issue](https://github.com/CyanCoding/Project-Tracker/issues)!
All feature requests should be tagged with the `enhancement` tag.


If you are intending to implement the feature, go ahead and read 
[Backend contribution](https://github.com/CyanCoding/Project-Tracker/new/master#submitting-a-bug-report)
**and** 
[Frontend contribution](https://github.com/CyanCoding/Project-Tracker/blob/master/CONTRIBUTING.md#frontend-contribution). 
Even if you're only intending to code backend
or frontend, it's important that you know what is required of any kind of
code contribution. When you create the issue, **make sure to assign yourself**. 
Any issue left unassigned will typically be assigned to the owner, [@CyanCoding](https://github.com/CyanCoding).

### Backend contribution
You're interested in coding part of the Project Tracker? You're in the right
starting place. The Project Tracker is a [WPF](https://docs.microsoft.com/en-us/dotnet/framework/wpf/)
`C#` project and all backend code is written in `C#`. All backend files
can be recognized by the `*.cs` extension.
Head over to the [issues](https://github.com/CyanCoding/Project-Tracker/issues)
and find something that you would like to work on **and assign yourself**. 
When you finish coding, submit your pull request and it will be reviewed.

Please only work on **one** bug report/feature at a time. The smaller the
pull request, the better and the more likely it is to be accepted.

### Frontend contribution
A frontend contribution changes the styling and/or design of the Project Tracker.
Frontend files can be recognized by the `*.xaml` extension. If you want to change
the design of the Project Tracker, please submit a pull request and state
how your contribution improves upon the overall layout **and** display
of the program. You do **not** need to create an issue unless the
frontend contribution fixes a bug.

To avoid any unwanted results, it's **heavily** advised that you make sure
any `C#` files related to the display you're changing don't interfere
with your design. Many components of the display are changed in the
`*.xaml.cs` files and you may need to change the respective `C#` file as well.

### Fixing documentation
If you notice a grammatical error or some other documentation-related
error, please edit the file and make a pull request. Do **not** create
an issue about it.
