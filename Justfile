CMD_ENV := if path_exists('/.dockerenv') == "false" { 'docker run --rm -u $(id -u) -v $(pwd):/workspaces/KlabTranslations -w /workspaces/KlabTranslations klabtranslations-toolchain' } else { '' }

build-ci-docker UID='1000':
    docker build --build-arg UID={{UID}} --target ci --tag klabtranslations-toolchain -f Dockerfile .

# build the project
build configuration='Debug' *args='':
    {{CMD_ENV}} dotnet build \
        --configuration {{configuration}} \
        {{args}}

release version='0.0.0':
    just build Releae /p:Version={{version}}

# clean the project
clean:
    {{CMD_ENV}} dotnet clean
    find . -type d -name 'bin' -exec rm -rf {} +
    find . -type d -name 'obj' -exec rm -rf {} +

# run the tests
test reportPath="./tmp" *args='':
    #!/usr/bin/env bash
    {{CMD_ENV}} dotnet-coverage \
        collect -f xml \
        -o {{reportPath}}/coverage.xml \
        "dotnet test --logger:junit;MethodFormat=Class;LogFilePath={{reportPath}}/{assembly}.results.xml {{args}}"
    
    
    CONTAINER_PATH=/workspaces/KlabTranslations
    HOST_PATH=$(pwd) # Assumes running from project root
    
    # Replace absolute source paths with relative paths in the coverage XML
    echo "Adjusting coverage report paths..."
    # sed -i "s|path=\"${CONTAINER_PATH}/|path=\"${HOST_PATH}/|g" {{reportPath}}/coverage.xml
    sed -i "s|path=\"${CONTAINER_PATH}/|path=\"|g" {{reportPath}}/coverage.xml

# format the code using dotnet format and the .editorconfig file
format *args:
    {{CMD_ENV}} dotnet format --no-restore -v diag {{args}}

# check the code format using dotnet format and the .editorconfig file
check-format:
    just format --verify-no-changes

publish *args:
    {{CMD_ENV}} dotnet nuget push **/*.nupkg --api-key "$SECRETS_NUGETORG_API" --source https://api.nuget.org/v3/index.json {{args}}

ru: run-uiex
run-uiex:
    dotnet run --project samples/KlabTranslations.AvaloniaExample/KlabTranslations.AvaloniaExample.csproj
