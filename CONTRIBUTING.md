# Contributing

Thank you for taking the time to contribute. All types of contributions are encouraged and valued. See the [Table of Contents](#table-of-contents) for the different ways to help and how this project handles them, and read the relevant section before making your contribution. It makes things easier for the maintainers and smoother for everyone involved.

> If you like the project but do not have time to contribute, that is fine. There are other easy ways to support it:
>
> - Star the project
> - Refer to it in your own project's README
> - Mention it to your friends and colleagues

## Table of Contents

- [I Have a Question](#i-have-a-question)
- [I Want To Contribute](#i-want-to-contribute)
- [Reporting Bugs](#reporting-bugs)
- [Suggesting Enhancements](#suggesting-enhancements)
- [Submitting Changes](#submitting-changes)

## I Have a Question

Before you ask a question, read the [README](README.md) and search the repository's Issues tab for existing issues that might help. If you find a suitable issue and still need clarification, ask your question there. It is also worth searching the internet for answers first.

If you still need clarification:

- Open a new issue from the repository's Issues tab.
- Provide as much context as you can about what you are running into.
- Provide project and platform versions (runtime, operating system and so on), depending on what seems relevant.

We will then look at the issue as soon as possible.

## I Want To Contribute

> ### Legal Notice
>
> When contributing to this project, you must agree that you have authored 100% of the content, that you have the necessary rights to the content and that the content you contribute may be provided under the project licence.

### Reporting Bugs

#### Before Submitting a Bug Report

A good bug report should not leave others needing to chase you for more information. Please investigate carefully, collect information and describe the issue in detail in your report. Completing the following steps in advance helps us fix any potential bug as fast as possible.

- Make sure that you are using the latest version.
- Read the [README](README.md) and check that the behaviour is a bug and not a problem with your environment, such as incompatible component versions. If you are looking for support, see [I Have a Question](#i-have-a-question).
- Search the repository's Issues tab, including closed issues, to see whether another user has already reported the same bug or error.
- Search the internet (including Stack Overflow) to see whether users outside the GitHub community have discussed the issue.
- Collect information about the bug:
  - Stack trace or error output, if there is any
  - Operating system, platform and version (Windows, Linux, macOS, x86, ARM)
  - Version of the interpreter, compiler, SDK, runtime environment or package manager, depending on what seems relevant
  - Your input and the output, where relevant
  - Whether you can reliably reproduce the issue, and whether you can also reproduce it with older versions

#### How Do I Submit a Good Bug Report?

> Do not report security vulnerabilities in public, including in the issue tracker. Follow the [security policy](SECURITY.md) instead.

We use GitHub issues to track bugs and errors. If you run into an issue with the project:

- Open a new issue from the repository's Issues tab.
- Explain the behaviour you expected and the actual behaviour.
- Provide as much context as possible and describe the *reproduction steps* that someone else can follow to recreate the issue on their own. This usually includes your code. For good bug reports you should isolate the problem and create a reduced test case.
- Provide the information you collected in the previous section.

Once it is filed:

- The project team will label the issue accordingly.
- A team member will try to reproduce the issue with your reproduction steps. If there are no reproduction steps, or no obvious way to reproduce the issue, the team will ask you for them. An issue that cannot be reproduced may not be addressed until it can be.
- If the team can reproduce the issue, it will be left for someone to implement, and you are welcome to contribute the fix yourself: see [Submitting Changes](#submitting-changes).

### Suggesting Enhancements

This section guides you through submitting an enhancement suggestion for this project, **including completely new features and minor improvements to existing functionality**. Following these guidelines helps maintainers and the community to understand your suggestion and find related suggestions.

#### Before Submitting an Enhancement

- Make sure that you are using the latest version.
- Read the [README](README.md) carefully and find out whether the functionality is already covered, perhaps by configuration.
- Search the repository's Issues tab to see whether the enhancement has already been suggested. If it has, add a comment to the existing issue instead of opening a new one.
- Find out whether your idea fits the scope and aims of the project. It is up to you to make a strong case for the merits of the feature. Keep in mind that we want features that will be useful to the majority of our users and not just a small subset.

#### How Do I Submit a Good Enhancement Suggestion?

Enhancement suggestions are tracked as GitHub issues.

- Use a **clear and descriptive title** for the issue to identify the suggestion.
- Provide a **step-by-step description of the suggested enhancement** in as much detail as possible.
- **Describe the current behaviour** and **explain which behaviour you expected to see instead**, and why. At this point you can also say which alternatives do not work for you.
- You may want to **include screenshots or screen recordings** that help you demonstrate the steps or point out the part the suggestion relates to.
- **Explain why this enhancement would be useful** to most users of the project. You may also want to point out other projects that solved it better and could serve as inspiration.

### Submitting Changes

Code and documentation changes are welcome, and documentation-only changes follow the same process.

- Work against an issue. For anything beyond a trivial fix, if there is not one for your change, open one first so the change can be discussed before you spend time on it.
- Branch from `main`, and keep each pull request to one logical change.
- Write commit messages in Conventional Commits format and reference the issue number.
- Run the repository's pre-commit hooks and linters before you push (see the [README](README.md)). They must pass.
- Add or update tests for any change in behaviour.
- If the repository keeps a changelog, add an entry using the repository's own tooling.
- Open the pull request as a draft while it is in progress, and mark it ready for review once the automated checks pass.
- Write prose, such as documentation, comments and commit messages, in UK English.
