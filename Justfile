set windows-powershell

# build the project
build configuration='Debug' *args='':
    dotnet build -c {{configuration}}

release version='0.0.0':
    just build Releae /p:Version={{version}}

# clean the project
clean:
    dotnet clean
    find . -name bin -type d -exec rm -rf {} \;
    find . -name obj -type d -exec rm -rf {} \;

# run the tests
test:
    dotnet run --project tests/Klab.Translations.Core.UnitTests/Klab.Translations.Core.UnitTests.csproj

# format the code using dotnet format and the .editorconfig file
format *args:
    dotnet format -v diag {{args}}

# check the code format using dotnet format and the .editorconfig file
check-format:
    just format --verify-no-changes
