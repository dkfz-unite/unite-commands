# Command Service API

## GET: [api](http://localhost:5300/api)
Health check.

### Responses
`"2022-03-17T09:45:10.9359202Z"` - Current UTC date and time in JSON format, if service is up and running

## POST: [api/run?key=[key]](http://localhost:5300/api/run?key=abc)
Run configured command.

### Parameters
`key` - Process key. Can be used to identify the process. All entries of `{proc}` in command or it's arguments will be replaced by the process key.

### Responses
- `200` - request was processed successfully
- `400` - request error with one of the following code
    - `1` - general error
    - `2` - processes limit exided