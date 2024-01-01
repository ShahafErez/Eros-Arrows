import { Component, Input } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { Member } from 'src/app/_models/member';
import { MembersService } from './../../_services/members.service';
import { PresenceService } from './../../_services/presence.service';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css'],
})
export class MemberCardComponent {
  @Input() member: Member | undefined;

  constructor(
    private MembersService: MembersService,
    private toastr: ToastrService,
    public presenceService: PresenceService
  ) {}

  addLike(member: Member) {
    this.MembersService.addLike(member.userName).subscribe({
      next: () => this.toastr.success('Added like to ' + member.userName),
    });
  }
}
