FROM mcr.microsoft.com/dotnet/sdk:9.0 AS ci

ARG UID
RUN <<EOF
    apt-get update
    apt-get install -y sudo curl

    useradd -m dev -u ${UID}
    echo "dev ALL=(ALL) NOPASSWD:ALL" >> /etc/sudoers

    curl --proto '=https' --tlsv1.2 -sSf https://just.systems/install.sh | bash -s -- --to /usr/local/bin

    # cleanup
    apt-get clean
    rm -rf /var/lib/apt/lists/*
EOF

# Install dotnet tools as user, because permissions
USER dev
ENV PATH="/home/dev/.dotnet/tools:${PATH}"
RUN dotnet tool install --global dotnet-coverage

# Switch back to root for the next stage setup if needed
USER root

FROM ci AS development

RUN <<EOF
    set -eu
    apt update
    apt install -y zsh git curl sudo pkg-config libssl-dev ssh

    # Setup shell for dev user
    chsh -s /bin/zsh dev
    su - dev -c 'sh -c "$(curl -fsSL https://raw.github.com/ohmyzsh/ohmyzsh/master/tools/install.sh)" "" --unattended'
    su - dev -c 'git clone https://github.com/zsh-users/zsh-autosuggestions ${ZSH_CUSTOM:-~/.oh-my-zsh/custom}/plugins/zsh-autosuggestions'
    su - dev -c 'sed -i "s/plugins=(git)/plugins=(git zsh-autosuggestions)/" ~/.zshrc'
    curl -sS https://starship.rs/install.sh | sh -s -- --yes
    su - dev -c 'echo "eval \"\$(starship init zsh)\"" >> ~/.zshrc'

    # UI Stuff
    apt install -y libice6 libsm6 libfontconfig1 libgdiplus

    # Install dotnet tools / templates
    dotnet new install xunit.v3.templates

    # cleanup
    apt-get clean
    rm -rf /var/lib/apt/lists/*
EOF

