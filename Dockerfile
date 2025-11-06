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

    # cleanup
    apt-get clean
    rm -rf /var/lib/apt/lists/*
EOF

