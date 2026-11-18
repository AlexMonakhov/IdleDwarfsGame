#!/usr/bin/env python3
"""Отправляет письмо-приглашение docs/email/invite.html через Gmail SMTP.

Нужен пароль приложения Google (https://myaccount.google.com/apppasswords, требует включённую 2FA),
обычный пароль от почты не подойдёт. Логин и пароль берутся из переменных окружения:

    export GMAIL_USER=you@gmail.com
    export GMAIL_APP_PASSWORD='abcd efgh ijkl mnop'

    python3 scripts/send-invite.py --to friend@mail.com --inviter Торин \
        --url https://game.example/invite/abc123 \
        --bg-url https://cdn.example/invite-bg.jpg

Фон должен лежать по публичному https-URL — Gmail не показывает картинки с диска или из вложений в CSS-фоне.
--dry-run сохраняет готовый HTML рядом с шаблоном (invite.rendered.html) и ничего не отправляет.
"""
import argparse
import os
import smtplib
import sys
from email.message import EmailMessage
from email.utils import formataddr
from pathlib import Path

TEMPLATE = Path(__file__).resolve().parent.parent / "docs" / "email" / "invite.html"


def render(inviter: str, url: str, bg_url: str) -> str:
    html = TEMPLATE.read_text(encoding="utf-8")
    for key, value in {"InviterName": inviter, "InviteUrl": url, "BackgroundUrl": bg_url}.items():
        html = html.replace("{{" + key + "}}", value)
    if "{{" in html:
        sys.exit("В шаблоне остались незаполненные плейсхолдеры {{...}}")
    return html


def plain_text(inviter: str, url: str) -> str:
    return (
        f"{inviter} зовёт тебя в поход в Idle Dwarfs!\n\n"
        "Собери отряд, спустись в шахты и сразись с боссами за древние кристаллы.\n\n"
        f"Войти в игру: {url}\n"
    )


def main() -> None:
    p = argparse.ArgumentParser(description="Отправить приглашение в Idle Dwarfs через Gmail")
    p.add_argument("--to", required=True, nargs="+", help="адрес(а) получателей")
    p.add_argument("--inviter", required=True, help="имя пригласившего")
    p.add_argument("--url", required=True, help="ссылка-приглашение для кнопки")
    p.add_argument("--bg-url", required=True, help="публичный https-URL картинки фона")
    p.add_argument("--subject", default="Тебя зовут в подземелья Idle Dwarfs ✦")
    p.add_argument("--from-name", default="Idle Dwarfs")
    p.add_argument("--dry-run", action="store_true", help="только сохранить HTML, не отправлять")
    args = p.parse_args()

    html = render(args.inviter, args.url, args.bg_url)

    if args.dry_run:
        out = TEMPLATE.with_name("invite.rendered.html")
        out.write_text(html, encoding="utf-8")
        print(f"Сохранено: {out}")
        return

    user = os.environ.get("GMAIL_USER")
    password = os.environ.get("GMAIL_APP_PASSWORD")
    if not user or not password:
        sys.exit("Задай GMAIL_USER и GMAIL_APP_PASSWORD (пароль приложения Google)")

    with smtplib.SMTP_SSL("smtp.gmail.com", 465) as smtp:
        smtp.login(user, password.replace(" ", ""))
        for to in args.to:
            msg = EmailMessage()
            msg["Subject"] = args.subject
            msg["From"] = formataddr((args.from_name, user))
            msg["To"] = to
            msg.set_content(plain_text(args.inviter, args.url))
            msg.add_alternative(html, subtype="html")
            smtp.send_message(msg)
            print(f"Отправлено: {to}")


if __name__ == "__main__":
    main()
