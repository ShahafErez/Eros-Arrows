import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
})
export class HomeComponent {
  constructor(public accountService: AccountService, private router: Router) {}

  navigateToRegister() {
    this.router.navigateByUrl('/register');
  }

  navigateToWhoLikesYou() {
    this.router.navigateByUrl('/lists');
  }

  navigateToMatches() {
    this.router.navigateByUrl('/members');
  }

  navigateToMessages() {
    this.router.navigateByUrl('/messages');
  }
}
