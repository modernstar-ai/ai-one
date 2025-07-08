## Process to create a pull request

#1 Create a branch for your pull request

Suggestion: Make a descriptive name for your branch.
For changes to UI, consider starting the branch name with 'ui-'

```
git branch ui-sidebar
```

change to the branch

```
git checkout ui-sidebar
```

push the branch to the remote repository

```
git push -u origin ui-sidebar
```

## To Process to test a pull request locally

1. Identify the Pull Request Number. this is the number displayed next to the PRs title in GitHub

2. Fetch the PR and create a new branch

```
gh pr checkout PR_NUMBER
-- OR --
git fetch origin pull/PR_NUMBER/head:BRANCH_NAME
```

e.g. `git fetch origin pull/13/head:PR-13` OR `gh pr checkout 13`

3. Switch to the new branch

```
git checkout BRANCH_NAME
```

e.g. `git checkout PR-13`

4. Merge changes from Main into the PR Branch

```
git checkout BRANCH_NAME
git merge main
```

5. Push changes back to the PR

```
git push origin BRANCH_NAME
```
