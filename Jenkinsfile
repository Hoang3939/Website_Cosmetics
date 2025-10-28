pipeline {
    agent { label 'github-server' }

    options {
        skipStagesAfterUnstable()
        skipDefaultCheckout(true)
        timestamps()
    }

    tools {
        git 'Default'
    }

    environment {
        OUT_DIR    = 'publish'
        DEPLOY_DIR = '/data/cosmetics/Website_Cosmetics/run'
        DLL        = 'Website_Cosmetics.dll'
        PORT       = '5001'
        SERVICE    = 'cosmetics-website.service'
    }

    stages {

        stage('Git safe.directory') {
            steps {
                sh """#!/usr/bin/env bash
                    set -euxo pipefail
                    git --version
                    git config --global --add safe.directory "\$WORKSPACE" || true
                """
            }
        }

        stage('Checkout') {
            steps {
                checkout([
                    $class: 'GitSCM',
                    branches: [[name: '*/feature/user-authentication']],
                    userRemoteConfigs: [[
                        url: 'https://github.com/your-username/Website_Cosmetics.git',
                        credentialsId: 'jenkins-github-user'
                    ]]
                ])
            }
        }

        stage('Build') {
            steps {
                sh """#!/usr/bin/env bash
                    set -euxo pipefail
                    cd Website_Cosmetics
                    dotnet --info
                    dotnet restore
                    dotnet build --no-restore -c Release
                """
            }
        }

        stage('Publish') {
            steps {
                sh """#!/usr/bin/env bash
                    set -euxo pipefail
                    cd Website_Cosmetics
                    dotnet publish Website_Cosmetics.csproj --no-restore -c Release -o "\$OUT_DIR" --nologo
                """
            }
            post {
                success {
                    archiveArtifacts artifacts: "${env.OUT_DIR}/**/*", fingerprint: true
                }
            }
        }

        stage('Deploy') {
            // SỬ DỤNG CREDENTIALS THAY VÌ HARDCODE PASSWORD
            environment {
                DB_CONNECTION = credentials('COSMETICS-DB-CONNECTION')
                EMAIL_PASSWORD = credentials('COSMETICS-EMAIL-PASSWORD')
            }
            steps {
                sh """#!/usr/bin/env bash
                    set -euxo pipefail

                    echo "WORKSPACE: \$WORKSPACE"
                    echo "PUB: \$WORKSPACE/\$OUT_DIR"
                    echo "DEPLOY_DIR: \$DEPLOY_DIR"
                    PUB="\$WORKSPACE/\$OUT_DIR"

                    if [[ -z "\$DEPLOY_DIR" || "\$DEPLOY_DIR" == "/" ]]; then
                        echo "Invalid DEPLOY_DIR: '\$DEPLOY_DIR'" >&2
                        exit 1
                    fi

                    sudo mkdir -p "\$DEPLOY_DIR"
                    sudo rm -rf -- "\$DEPLOY_DIR"/*
                    sudo cp -a "\$PUB/." "\$DEPLOY_DIR/"
                    sudo chown -R jenkins:jenkins "\$DEPLOY_DIR"

                    sudo tee /etc/systemd/system/\$SERVICE > /dev/null <<EOF
[Unit]
Description=Cosmetics Website (.NET 8)
After=network.target

[Service]
WorkingDirectory=\$DEPLOY_DIR
Environment="ASPNETCORE_ENVIRONMENT=Production"
Environment="ASPNETCORE_URLS=http://0.0.0.0:\$PORT"

# --- CONFIG OVERRIDES (PHẢI KHỚP VỚI APPSETTINGS.JSON) ---
Environment="ConnectionStrings__DefaultConnection=\$DB_CONNECTION"

Environment="Email__SmtpHost=smtp.gmail.com"
Environment="Email__SmtpPort=587"
Environment="Email__Username=hoangkutecm39@gmail.com"
Environment="Email__Password=\$EMAIL_PASSWORD"
Environment="Email__FromEmail=hoangkutecm39@gmail.com"
Environment="Email__FromName=Cosmetic Store"
Environment="Email__BaseUrl=http://34.142.130.206:\$PORT"
# --- KẾT THÚC OVERRIDES ---

User=jenkins
Group=jenkins
ExecStart=/usr/bin/dotnet \$DEPLOY_DIR/\$DLL
Restart=always
RestartSec=5
SyslogIdentifier=cosmetics-website

[Install]
WantedBy=multi-user.target
EOF

                    sudo systemctl daemon-reload
                    sudo systemctl enable "\$SERVICE"
                    sudo systemctl restart "\$SERVICE"

                    echo "==== SERVICE STATUS ===="
                    sudo systemctl --no-pager -l status "\$SERVICE" || true

                    # Logic kiểm tra port chặt chẽ hơn
                    echo "==== WAITING PORT (max 30s) ===="
                    for i in {1..30}; do
                        # SỬA DÒNG NÀY: Dùng grep đơn giản nhất, bỏ -w
                        if sudo ss -ltn | grep -q ":\$PORT"; then 
                            echo "Port \$PORT is listening."
                            # In ra dòng khớp để xem chi tiết
                            sudo ss -ltn | grep ":\$PORT" 
                            break
                        fi
                        if [[ "\$i" -eq 30 ]]; then
                            echo "LỖI: Port \$PORT không mở sau 30 giây! (Lệnh grep có thể đang không hoạt động đúng)" >&2
                            # Vẫn giữ lại log để phòng trường hợp lỗi khác
                            sudo journalctl -u "\$SERVICE" -n 100 --no-pager || true 
                            exit 1
                        fi
                        echo "Chờ port \$PORT... (thử \$i/30)"
                        sleep 1
                    done

                    echo "==== LAST 80 LOGS (journalctl) ===="
                    sudo journalctl -u "\$SERVICE" -n 80 --no-pager || true
                """
            }
        }
    }
}
