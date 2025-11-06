CMD_ENV := if path_exists('/.dockerenv') == "false" { 'docker run --rm -u $(id -u) -v $(pwd):/workspaces/KlabTranslations -w /workspaces/KlabTranslations klabtranslations-toolchain' } else { '' }

build-ci-docker UID='1000':
    docker build --build-arg UID={{UID}} --target ci --tag klabtranslations-toolchain -f Dockerfile .

# build the project
build configuration='Debug' *args='':
    {{CMD_ENV}} dotnet build -c {{configuration}} {{args}}

release version='0.0.0':
    just build Releae /p:Version={{version}}

# clean the project
clean:
    {{CMD_ENV}} dotnet clean

# run the tests
test configuration='Debug' *args='':
    {{CMD_ENV}} dotnet test -c {{configuration}} {{args}}

# format the code using dotnet format and the .editorconfig file
format *args:
    {{CMD_ENV}} dotnet format --no-restore -v diag {{args}}

# check the code format using dotnet format and the .editorconfig file
check-format:
    just format --verify-no-changes
