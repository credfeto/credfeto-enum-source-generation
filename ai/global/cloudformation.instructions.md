# CloudFormation Instructions

> Load when: any `cloudformation*.json` file or other CloudFormation template is present or being changed.

[Back to Global Instructions Index](index.md)

## Mappings Over Parameters (MANDATORY)

- Put every template value in a mapping (a `Settings` map with a `Values` key, read with `Fn::FindInMap`) unless the deploy action (`.github/actions/cloudformation-deploy/action.yml`) explicitly passes it as a parameter.
- Declare a parameter only when the deploy action supplies it **and** the template uses it. The action supplies only the keys listed in its `Build TemplateParameters file` step (`ApplicationName`, `ApplicationContainer`, `ApplicationVersion`, `ApiPrefix`, `ApiHost`, `AwsContainerRegistry`, `LoadBalancerHostName`, `LoadBalancerPriority`, `Direct1LoadBalancerPriority`, `Direct2LoadBalancerPriority`), and skips any whose value is empty; read the action itself for the current list.
- Reason: `aws cloudformation deploy` keeps an existing stack's previous value for every parameter the action does not pass. A changed parameter `Default` therefore never reaches a live stack, while a changed mapping value is part of the template and is applied on the next deploy.

```json
{
  "Mappings": {
    "Settings": {
      "Values": {
        "LogRetentionDays": "30"
      }
    }
  },
  "Resources": {
    "LogGroup": {
      "Type": "AWS::Logs::LogGroup",
      "Properties": {
        "RetentionInDays": { "Fn::FindInMap": ["Settings", "Values", "LogRetentionDays"] }
      }
    }
  }
}
```

## Substituting Mapping Values

- Short-form `Fn::Sub` (a single string) can only read parameters, resource attributes and pseudo parameters, not mappings. When a substituted string needs a mapping value (for example a Secrets Manager secret ARN), use long-form `Fn::Sub` with a variable map whose entries come from `Fn::FindInMap`.

```json
{
  "Fn::Sub": [
    "arn:aws:secretsmanager:${AWS::Region}:${AWS::AccountId}:secret:${SecretName}",
    {
      "SecretName": { "Fn::FindInMap": ["Settings", "Values", "SecretName"] }
    }
  ]
}
```

## Naming Existing AWS Resources

- Before a template names an existing AWS resource (for example an IAM role, a cluster or a secret), check the exact name against the sibling service templates that already use it; never infer a name from a naming pattern. A wrong name fails only at deploy time, against the live account.
